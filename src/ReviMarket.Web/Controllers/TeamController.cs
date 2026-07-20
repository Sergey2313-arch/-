using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Controllers;

[Authorize(Roles = UserRoles.Admin + "," + UserRoles.Manager + "," + UserRoles.Owner + "," + UserRoles.CoOwner)]
public class TeamController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public TeamController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.OrderByDescending(x => x.CreatedAt).ToListAsync();
        ViewBag.Roles = UserRoles.All;
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(string userId, string role)
    {
        if (!UserRoles.All.Contains(role)) return BadRequest();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        if (!await CanManageUserAsync(user))
        {
            TempData["TeamNotice"] = "Нельзя изменить роль этого пользователя текущими правами.";
            return RedirectToAction(nameof(Index));
        }

        if (!CanAssignRole(role))
        {
            TempData["TeamNotice"] = "Недостаточно прав для назначения этой роли.";
            return RedirectToAction(nameof(Index));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.Where(x => UserRoles.All.Contains(x) && x != role).ToList();

        if (!currentRoles.Contains(role))
        {
            var addResult = await _userManager.AddToRoleAsync(user, role);
            if (!addResult.Succeeded)
            {
                TempData["TeamNotice"] = "Не удалось назначить роль.";
                return RedirectToAction(nameof(Index));
            }
        }

        if (rolesToRemove.Any())
        {
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
        }

        user.AccountType = role;
        await _userManager.UpdateAsync(user);
        TempData["TeamNotice"] = $"Роль пользователя {user.DisplayName} изменена на {role}.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CanManageUserAsync(ApplicationUser target)
    {
        var currentUserId = _userManager.GetUserId(User);
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
        return user.AccountType == role || await _userManager.IsInRoleAsync(user, role);
    }
}
