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

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var user = await _userManager.GetUserAsync(User);

        ViewBag.User = user;
        ViewBag.MyOrdersCount = await _db.MarketItems.CountAsync(x => x.OwnerId == userId && x.Type == MarketItemTypes.Order);
        ViewBag.MyProductsCount = await _db.MarketItems.CountAsync(x => x.OwnerId == userId && x.Type == MarketItemTypes.Product);
        ViewBag.AllOrdersCount = await _db.MarketItems.CountAsync(x => x.Type == MarketItemTypes.Order);
        ViewBag.AllProductsCount = await _db.MarketItems.CountAsync(x => x.Type == MarketItemTypes.Product);
        ViewBag.MessagesCount = await _db.ChatMessages.CountAsync(x => x.SenderId == userId || x.ReceiverId == userId);

        return View();
    }
}
