using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Controllers;

[Authorize(Roles = UserRoles.Admin)]
public class AdminQueueController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminQueueController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _db.MarketItems
            .Include(x => x.Owner)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return View(items);
    }
}
