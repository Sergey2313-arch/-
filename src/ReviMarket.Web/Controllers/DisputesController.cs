using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Controllers;

[Authorize]
public class DisputesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public DisputesController(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Open(int dealId)
    {
        var uid = _users.GetUserId(User)!;
        var deal = await _db.Deals.FirstOrDefaultAsync(x => x.Id == dealId && (x.CustomerId == uid || x.ExecutorId == uid));
        if (deal is null) return NotFound();
        if (deal.Status == DealStatuses.Completed || deal.Status == DealStatuses.Refunded) return BadRequest();

        deal.Status = DealStatuses.Dispute;
        _db.OrderCases.Add(new OrderCase
        {
            MarketItemId = deal.MarketItemId,
            CreatedById = uid,
            Title = $"Спор по сделке #{deal.Id}",
            Text = "Открыт спор по сделке. Средства остаются в холде до решения администрации."
        });

        await _db.SaveChangesAsync();
        return RedirectToAction("Index", "Deals");
    }
}
