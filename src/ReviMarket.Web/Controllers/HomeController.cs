using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ProductsCount = await _db.MarketItems.CountAsync(x => x.Type == MarketItemTypes.Product);
        ViewBag.OrdersCount = await _db.MarketItems.CountAsync(x => x.Type == MarketItemTypes.Order);
        return View();
    }
}
