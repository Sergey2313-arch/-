using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;
using ReviMarket.Web.ViewModels;

namespace ReviMarket.Web.Controllers;

[Authorize(Roles = UserRoles.Admin + "," + UserRoles.Manager + "," + UserRoles.Owner + "," + UserRoles.CoOwner + "," + UserRoles.Moderator)]
public class ModerationController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public ModerationController(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _db.MarketItems
            .Include(x => x.Owner)
            .Include(x => x.AssignedExecutor)
            .Where(x => x.Type == MarketItemTypes.Order)
            .OrderBy(x => x.ReviewStatus == ReviewStatuses.Pending ? 0 : x.ReviewStatus == ReviewStatuses.Blocked ? 1 : 2)
            .ThenByDescending(x => x.CreatedAt)
            .Take(100)
            .ToListAsync();

        var messages = await _db.ChatMessages
            .Include(x => x.Sender)
            .Include(x => x.Receiver)
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .ToListAsync();

        var users = await _users.Users
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .ToListAsync();

        var supportRequests = await _db.SupportRequests
            .Include(x => x.User)
            .Include(x => x.Agent)
            .Where(x => x.Status != CaseStatuses.Done)
            .OrderByDescending(x => x.CreatedAt)
            .Take(30)
            .ToListAsync();

        var orderCases = await _db.OrderCases
            .Include(x => x.MarketItem)
            .Include(x => x.CreatedBy)
            .Include(x => x.Agent)
            .Where(x => x.Status != CaseStatuses.Done)
            .OrderByDescending(x => x.CreatedAt)
            .Take(50)
            .ToListAsync();

        return View(new ModerationDashboardViewModel
        {
            Orders = orders,
            Messages = messages,
            Users = users,
            OrderCases = orderCases,
            SupportRequests = supportRequests
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveOrder(int id)
    {
        var item = await _db.MarketItems.FirstOrDefaultAsync(x => x.Id == id && x.Type == MarketItemTypes.Order);
        if (item is null) return NotFound();

        item.ReviewStatus = ReviewStatuses.Approved;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnOrder(int id)
    {
        var item = await _db.MarketItems.FirstOrDefaultAsync(x => x.Id == id && x.Type == MarketItemTypes.Order);
        if (item is null) return NotFound();

        item.ReviewStatus = ReviewStatuses.Pending;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BlockOrder(int id)
    {
        var item = await _db.MarketItems.FirstOrDefaultAsync(x => x.Id == id && x.Type == MarketItemTypes.Order);
        if (item is null) return NotFound();

        item.ReviewStatus = ReviewStatuses.Blocked;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var message = await _db.ChatMessages.FirstOrDefaultAsync(x => x.Id == id);
        if (message is null) return NotFound();

        _db.ChatMessages.Remove(message);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LockUser(string id)
    {
        var currentUserId = _users.GetUserId(User);
        if (string.IsNullOrWhiteSpace(id) || id == currentUserId) return BadRequest();

        var user = await _users.FindByIdAsync(id);
        if (user is null) return NotFound();

        await _users.SetLockoutEnabledAsync(user, true);
        await _users.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        await _users.UpdateSecurityStampAsync(user);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnlockUser(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user is null) return NotFound();

        await _users.SetLockoutEndDateAsync(user, null);
        await _users.UpdateSecurityStampAsync(user);

        return RedirectToAction(nameof(Index));
    }
}
