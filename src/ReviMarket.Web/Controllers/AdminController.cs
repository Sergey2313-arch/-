using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;
using ReviMarket.Web.ViewModels;

namespace ReviMarket.Web.Controllers;

[Authorize(Roles = UserRoles.Owner + "," + UserRoles.CoOwner + "," + UserRoles.Admin + "," + UserRoles.Manager)]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTimeOffset.UtcNow;
        var users = await _users.Users
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var pendingOrders = await _db.MarketItems
            .Include(x => x.Owner)
            .Where(x => x.Type == MarketItemTypes.Order && x.ReviewStatus == ReviewStatuses.Pending)
            .OrderByDescending(x => x.CreatedAt)
            .Take(8)
            .ToListAsync();

        var userCases = await _db.UserCases
            .Include(x => x.CreatedBy)
            .Include(x => x.TargetUser)
            .Where(x => x.Status != CaseStatuses.Done)
            .OrderByDescending(x => x.CreatedAt)
            .Take(6)
            .ToListAsync();

        var orderCases = await _db.OrderCases
            .Include(x => x.MarketItem)
            .Include(x => x.CreatedBy)
            .Where(x => x.Status != CaseStatuses.Done)
            .OrderByDescending(x => x.CreatedAt)
            .Take(6)
            .ToListAsync();

        var supportRequests = await _db.SupportRequests
            .Include(x => x.User)
            .Include(x => x.Agent)
            .Where(x => x.Status != CaseStatuses.Done)
            .OrderByDescending(x => x.CreatedAt)
            .Take(6)
            .ToListAsync();

        var model = new AdminDashboardViewModel
        {
            UsersCount = users.Count,
            OrdersCount = await _db.MarketItems.CountAsync(x => x.Type == MarketItemTypes.Order),
            ProductsCount = await _db.MarketItems.CountAsync(x => x.Type == MarketItemTypes.Product),
            MessagesCount = await _db.ChatMessages.CountAsync() + await _db.SupportMessages.CountAsync(),
            SupportCount = await _db.SupportRequests.CountAsync(x => x.Status != CaseStatuses.Done),
            PendingOrdersCount = await _db.MarketItems.CountAsync(x => x.Type == MarketItemTypes.Order && x.ReviewStatus == ReviewStatuses.Pending),
            OpenReportsCount = await _db.UserCases.CountAsync(x => x.Status != CaseStatuses.Done) + await _db.OrderCases.CountAsync(x => x.Status != CaseStatuses.Done),
            LockedUsersCount = users.Count(x => x.LockoutEnd is not null && x.LockoutEnd > now),
            ActiveDealsCount = await _db.Deals.CountAsync(x => x.Status == DealStatuses.Funded || x.Status == DealStatuses.InProgress),
            Roles = UserRoles.All,
            Users = users
                .OrderByDescending(x => x.LockoutEnd is not null && x.LockoutEnd > now)
                .ThenByDescending(x => x.CreatedAt)
                .Take(80)
                .ToList(),
            PendingOrders = pendingOrders,
            UserCases = userCases,
            OrderCases = orderCases,
            SupportRequests = supportRequests
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(string userId, string role)
    {
        if (string.IsNullOrWhiteSpace(userId) || !UserRoles.All.Contains(role))
        {
            return BadRequest();
        }

        var user = await _users.FindByIdAsync(userId);
        if (user is null) return NotFound();

        if (!await CanManageUserAsync(user))
        {
            TempData["AdminNotice"] = "Нельзя изменить роль этого пользователя текущими правами.";
            return RedirectToAction(nameof(Index));
        }

        if (!CanAssignRole(role))
        {
            TempData["AdminNotice"] = "Недостаточно прав для назначения этой роли.";
            return RedirectToAction(nameof(Index));
        }

        var currentRoles = await _users.GetRolesAsync(user);
        if (!currentRoles.Contains(role))
        {
            var addResult = await _users.AddToRoleAsync(user, role);
            if (!addResult.Succeeded)
            {
                TempData["AdminNotice"] = "Не удалось назначить роль.";
                return RedirectToAction(nameof(Index));
            }
        }

        var rolesToRemove = currentRoles.Where(x => UserRoles.All.Contains(x) && x != role).ToList();
        if (rolesToRemove.Any())
        {
            var removeResult = await _users.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                TempData["AdminNotice"] = "Роль назначена, но старые роли не удалось снять полностью.";
                return RedirectToAction(nameof(Index));
            }
        }

        user.AccountType = role;
        var updateResult = await _users.UpdateAsync(user);
        TempData["AdminNotice"] = updateResult.Succeeded
            ? $"Роль пользователя {user.DisplayName} изменена на {role}."
            : "Роль в аккаунте не обновилась.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LockUser(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user is null) return NotFound();

        if (!await CanManageUserAsync(user))
        {
            TempData["AdminNotice"] = "Нельзя заблокировать этого пользователя текущими правами.";
            return RedirectToAction(nameof(Index));
        }

        await _users.SetLockoutEnabledAsync(user, true);
        await _users.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        await _users.UpdateSecurityStampAsync(user);

        TempData["AdminNotice"] = $"Пользователь {user.DisplayName} заблокирован.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnlockUser(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user is null) return NotFound();

        if (!await CanManageUserAsync(user))
        {
            TempData["AdminNotice"] = "Нельзя разблокировать этого пользователя текущими правами.";
            return RedirectToAction(nameof(Index));
        }

        await _users.SetLockoutEndDateAsync(user, null);
        await _users.UpdateSecurityStampAsync(user);

        TempData["AdminNotice"] = $"Пользователь {user.DisplayName} разблокирован.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CanManageUserAsync(ApplicationUser target)
    {
        var currentUserId = _users.GetUserId(User);
        if (target.Id == currentUserId)
        {
            return false;
        }

        if (await HasRoleOrAccountAsync(target, UserRoles.Owner))
        {
            return User.IsInRole(UserRoles.Owner);
        }

        if (await HasRoleOrAccountAsync(target, UserRoles.Admin))
        {
            return User.IsInRole(UserRoles.Owner);
        }

        if (await HasRoleOrAccountAsync(target, UserRoles.CoOwner))
        {
            return User.IsInRole(UserRoles.Owner) || User.IsInRole(UserRoles.Admin);
        }

        if (await HasRoleOrAccountAsync(target, UserRoles.Manager))
        {
            return User.IsInRole(UserRoles.Owner) || User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.CoOwner);
        }

        return true;
    }

    private bool CanAssignRole(string role)
    {
        if (role == UserRoles.Owner)
        {
            return User.IsInRole(UserRoles.Owner);
        }

        if (role == UserRoles.Admin || role == UserRoles.CoOwner)
        {
            return User.IsInRole(UserRoles.Owner);
        }

        if (role == UserRoles.Manager)
        {
            return User.IsInRole(UserRoles.Owner) || User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.CoOwner);
        }

        return true;
    }

    private async Task<bool> HasRoleOrAccountAsync(ApplicationUser user, string role)
    {
        return user.AccountType == role || await _users.IsInRoleAsync(user, role);
    }
}
