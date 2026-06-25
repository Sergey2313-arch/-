using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Controllers;

[Authorize(Roles = UserRoles.Admin + "," + UserRoles.Manager + "," + UserRoles.Moderator)]
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
            .Where(x => x.Type == MarketItemTypes.Order)
            .OrderBy(x => x.ReviewStatus == ReviewStatuses.Pending ? 0 : 1)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync();

        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var item = await _db.MarketItems.FirstOrDefaultAsync(x => x.Id == id && x.Type == MarketItemTypes.Order);
        if (item is null) return NotFound();
        item.ReviewStatus = ReviewStatuses.Approved;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnToQueue(int id)
    {
        var item = await _db.MarketItems.FirstOrDefaultAsync(x => x.Id == id && x.Type == MarketItemTypes.Order);
        if (item is null) return NotFound();
        item.ReviewStatus = ReviewStatuses.Pending;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(int id)
    {
        var item = await _db.MarketItems.FirstOrDefaultAsync(x => x.Id == id && x.Type == MarketItemTypes.Order);
        if (item is null) return NotFound();
        item.ReviewStatus = ReviewStatuses.Blocked;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
