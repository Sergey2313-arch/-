using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;
using ReviMarket.Web.Services.Security;

namespace ReviMarket.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly TextSecurityService _textSecurity;

    public ReportsController(ApplicationDbContext db, UserManager<ApplicationUser> users, TextSecurityService textSecurity)
    {
        _db = db;
        _users = users;
        _textSecurity = textSecurity;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserProfile(string id, string? reason)
    {
        var target = await _users.FindByIdAsync(id);
        if (target is null) return NotFound();

        var text = _textSecurity.CleanText(reason, 600);
        if (string.IsNullOrWhiteSpace(text))
        {
            text = "Пользователь отправил жалобу без комментария.";
        }

        _db.SupportRequests.Add(new SupportRequest
        {
            UserId = _users.GetUserId(User),
            Title = $"Жалоба на пользователя {target.DisplayName}",
            Text = text,
            Status = CaseStatuses.Open,
            Priority = "High",
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return RedirectToAction("Public", "Profile", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Order(int id, string? reason)
    {
        var order = await _db.MarketItems.FirstOrDefaultAsync(x => x.Id == id && x.Type == MarketItemTypes.Order);
        if (order is null) return NotFound();

        var text = _textSecurity.CleanText(reason, 600);
        if (string.IsNullOrWhiteSpace(text))
        {
            text = "Пользователь отправил жалобу без комментария.";
        }

        _db.SupportRequests.Add(new SupportRequest
        {
            UserId = _users.GetUserId(User),
            Title = $"Жалоба на заказ #{order.Id}: {order.Title}",
            Text = text,
            Status = CaseStatuses.Open,
            Priority = "High",
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return RedirectToAction("Details", "Orders", new { id });
    }
}
