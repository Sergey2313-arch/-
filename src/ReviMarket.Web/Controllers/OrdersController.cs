using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Controllers;

public class OrdersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, string? category)
    {
        var query = _db.MarketItems
            .Include(x => x.Owner)
            .Where(x => x.Type == MarketItemTypes.Order);

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(x => x.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Title.Contains(search) || x.Description.Contains(search));
        }

        ViewBag.Search = search;
        ViewBag.Category = category;
        ViewBag.Categories = MarketCategories.All;
        return View(await query.OrderByDescending(x => x.CreatedAt).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _db.MarketItems.Include(x => x.Owner).FirstOrDefaultAsync(x => x.Id == id && x.Type == MarketItemTypes.Order);
        if (item is null) return NotFound();
        return View(item);
    }

    [Authorize(Roles = UserRoles.Customer + "," + UserRoles.Admin)]
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Categories = MarketCategories.All;
        return View(new MarketItem { Type = MarketItemTypes.Order, Category = MarketCategories.Design });
    }

    [Authorize(Roles = UserRoles.Customer + "," + UserRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MarketItem item)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = MarketCategories.All;
            return View(item);
        }

        item.Type = MarketItemTypes.Order;
        item.OwnerId = _userManager.GetUserId(User);
        item.CreatedAt = DateTime.UtcNow;
        _db.MarketItems.Add(item);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
