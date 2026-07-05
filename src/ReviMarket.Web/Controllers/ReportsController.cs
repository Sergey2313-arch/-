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

        var userId = _users.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId) || userId == target.Id) return BadRequest();

        var text = _textSecurity.CleanText(reason, 600);
        if (string.IsNullOrWhiteSpace(text))
        {
            text = "Пользователь отправил жалобу без комментария.";
        }

        if (_textSecurity.LooksDangerous(text))
        {
            TempData["ReportInfo"] = "Жалоба похожа на вредоносный текст и не отправлена.";
            return RedirectToAction("Public", "Profile", new { id });
        }

        var duplicateOpenCase = await _db.UserCases.AnyAsync(x =>
            x.TargetUserId == target.Id
            && x.CreatedById == userId
            && x.Status != CaseStatuses.Done
            && x.Title.StartsWith("Жалоба на пользователя"));

        if (duplicateOpenCase)
        {
            TempData["ReportInfo"] = "Жалоба на этого пользователя уже отправлена и ждет проверки.";
            return RedirectToAction("Public", "Profile", new { id });
        }

        _db.SupportRequests.Add(new SupportRequest
        {
            UserId = userId,
            Title = $"Жалоба на пользователя {target.DisplayName}",
            Text = text,
            Status = CaseStatuses.Open,
            Priority = "High",
            CreatedAt = DateTime.UtcNow
        });

        _db.UserCases.Add(new UserCase
        {
            CreatedById = userId,
            TargetUserId = target.Id,
            Title = $"Жалоба на пользователя {target.DisplayName}",
            Text = text,
            Status = CaseStatuses.Open,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        TempData["ReportSuccess"] = "Жалоба отправлена. Модерация проверит профиль.";
        return RedirectToAction("Public", "Profile", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Order(int id, string? reportType, string? reason)
    {
        var order = await _db.MarketItems.FirstOrDefaultAsync(x => x.Id == id && x.Type == MarketItemTypes.Order);
        if (order is null) return NotFound();

        var userId = _users.GetUserId(User);
        var reportTypeLabel = GetOrderReportTypeLabel(reportType);
        var comment = _textSecurity.CleanText(reason, 600);
        var text = string.IsNullOrWhiteSpace(comment)
            ? $"Тип жалобы: {reportTypeLabel}. Пользователь не добавил комментарий."
            : $"Тип жалобы: {reportTypeLabel}. Комментарий: {comment}";

        if (_textSecurity.LooksDangerous(text))
        {
            TempData["ReportInfo"] = "Жалоба похожа на вредоносный текст и не отправлена.";
            return RedirectToAction("Details", "Orders", new { id });
        }

        var duplicateOpenCase = await _db.OrderCases.AnyAsync(x =>
            x.MarketItemId == order.Id
            && x.CreatedById == userId
            && x.Status != CaseStatuses.Done
            && x.Title.StartsWith("Жалоба на заказ"));

        if (duplicateOpenCase)
        {
            TempData["ReportInfo"] = "Жалоба по этому заказу уже отправлена и ждет проверки.";
            return RedirectToAction("Details", "Orders", new { id });
        }

        _db.SupportRequests.Add(new SupportRequest
        {
            UserId = userId,
            Title = $"Жалоба на заказ #{order.Id}: {reportTypeLabel}",
            Text = text,
            Status = CaseStatuses.Open,
            Priority = "High",
            CreatedAt = DateTime.UtcNow
        });

        _db.OrderCases.Add(new OrderCase
        {
            MarketItemId = order.Id,
            CreatedById = userId,
            Title = $"Жалоба на заказ #{order.Id}: {reportTypeLabel}",
            Text = text,
            Status = CaseStatuses.Open,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        TempData["ReportSuccess"] = "Жалоба отправлена. Поддержка и модерация проверят заказ.";
        return RedirectToAction("Details", "Orders", new { id });
    }

    private static string GetOrderReportTypeLabel(string? reportType) => reportType switch
    {
        "fraud" => "Мошенник или обман",
        "outside_payment" => "Просит оплату или контакты вне биржи",
        "spam" => "Спам или реклама",
        "illegal" => "Запрещенный или незаконный заказ",
        "abuse" => "Оскорбления, угрозы или давление",
        "copyright" => "Нарушение авторских прав",
        "wrong_category" => "Неверная категория или описание",
        _ => "Другое нарушение"
    };
}
