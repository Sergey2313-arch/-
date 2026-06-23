using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Controllers;

public class BoardController : Controller
{
    private readonly ApplicationDbContext _db;

    public BoardController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search, string? category)
    {
        var query = _db.MarketItems
            .Include(x => x.Owner)
            .Where(x => x.Type == MarketItemTypes.Vacancy && x.ReviewStatus == ReviewStatuses.Approved);

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
}
