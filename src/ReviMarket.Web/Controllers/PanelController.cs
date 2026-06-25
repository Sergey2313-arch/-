using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Controllers;

[Authorize]
public class PanelController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public PanelController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet("/Panel")]
    [HttpGet("/admin.html")]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var user = await _userManager.GetUserAsync(User);
        if (!CanOpenPanel(user)) return RedirectToAction("Index", "Profile");

        ViewBag.User = user;
        ViewBag.MyOrdersCount = await _db.MarketItems.CountAsync(x => x.OwnerId == userId && x.Type == MarketItemTypes.Order);
        ViewBag.TakenOrdersCount = await _db.MarketItems.CountAsync(x => x.AssignedExecutorId == userId && x.Type == MarketItemTypes.Order);
        ViewBag.AllOrdersCount = await _db.MarketItems.CountAsync(x => x.Type == MarketItemTypes.Order);
        ViewBag.InWorkOrdersCount = await _db.MarketItems.CountAsync(x => x.Type == MarketItemTypes.Order && x.OrderStatus == OrderStatuses.InWork);
        ViewBag.ActiveDealsCount = await _db.Deals.CountAsync(x => x.Status == DealStatuses.Funded || x.Status == DealStatuses.InProgress);
        ViewBag.MessagesCount = await _db.ChatMessages.CountAsync(x => x.SenderId == userId || x.ReceiverId == userId);

        return View();
    }

    private bool CanOpenPanel(ApplicationUser? user)
    {
        var role = user?.AccountType;
        return User.IsInRole(UserRoles.Admin)
            || User.IsInRole(UserRoles.Manager)
            || User.IsInRole(UserRoles.Owner)
            || User.IsInRole(UserRoles.CoOwner)
            || User.IsInRole(UserRoles.Moderator)
            || User.IsInRole(UserRoles.Lawyer)
            || User.IsInRole(UserRoles.SupportAgent)
            || role == UserRoles.Admin
            || role == UserRoles.Manager
            || role == UserRoles.Owner
            || role == UserRoles.CoOwner
            || role == UserRoles.Moderator
            || role == UserRoles.Lawyer
            || role == UserRoles.SupportAgent;
    }
}
