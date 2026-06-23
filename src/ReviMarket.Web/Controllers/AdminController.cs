using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Controllers;

[Authorize(Roles = UserRoles.Admin)]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.UsersCount = await _db.Users.CountAsync();
        ViewBag.OrdersCount = await _db.MarketItems.CountAsync(x => x.Type == MarketItemTypes.Order);
        ViewBag.ProductsCount = await _db.MarketItems.CountAsync(x => x.Type == MarketItemTypes.Product);
        ViewBag.MessagesCount = await _db.ChatMessages.CountAsync();
        return View();
    }
}
