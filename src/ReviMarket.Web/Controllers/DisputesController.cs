using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;
using ReviMarket.Web.ViewModels;

namespace ReviMarket.Web.Controllers;

[Authorize]
public class DisputesController : Controller
{
    private const string StaffRoles = UserRoles.Owner + "," + UserRoles.CoOwner + "," + UserRoles.Admin + "," + UserRoles.Manager + "," + UserRoles.Moderator + "," + UserRoles.Lawyer;

    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public DisputesController(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    [Authorize(Roles = StaffRoles)]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var deals = await _db.Deals
            .Include(x => x.Customer)
            .Include(x => x.Executor)
            .Include(x => x.MarketItem)
            .Where(x => x.Status == DealStatuses.Dispute)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var orderIds = deals
            .Where(x => x.MarketItemId is not null)
            .Select(x => x.MarketItemId!.Value)
            .Distinct()
            .ToList();

        var cases = orderIds.Any()
            ? await _db.OrderCases
                .Include(x => x.CreatedBy)
                .Include(x => x.Agent)
                .Where(x => x.MarketItemId != null && orderIds.Contains(x.MarketItemId.Value) && x.Status != CaseStatuses.Done)
                .OrderBy(x => x.Status == CaseStatuses.Open ? 0 : 1)
                .ThenByDescending(x => x.CreatedAt)
                .ToListAsync()
            : new List<OrderCase>();

        var items = deals
            .Select(deal => new DisputeQueueItemViewModel
            {
                Deal = deal,
                Case = cases.FirstOrDefault(x => x.MarketItemId == deal.MarketItemId)
            })
            .ToList();

        return View(new DisputeIndexViewModel
        {
            Items = items,
            OpenCount = items.Count(x => x.Case?.Status != CaseStatuses.InProgress),
            InProgressCount = items.Count(x => x.Case?.Status == CaseStatuses.InProgress)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Open(int dealId)
    {
        var uid = _users.GetUserId(User)!;
        var deal = await _db.Deals.FirstOrDefaultAsync(x => x.Id == dealId && (x.CustomerId == uid || x.ExecutorId == uid));
        if (deal is null) return NotFound();
        if (deal.Status == DealStatuses.Completed || deal.Status == DealStatuses.Refunded) return BadRequest();
        if (deal.Status == DealStatuses.Dispute) return RedirectToAction("Index", "Deals");

        deal.Status = DealStatuses.Dispute;

        var hasOpenCase = deal.MarketItemId is not null && await _db.OrderCases.AnyAsync(x =>
            x.MarketItemId == deal.MarketItemId
            && x.Status != CaseStatuses.Done
            && x.Title.StartsWith("Спор по сделке"));

        if (!hasOpenCase)
        {
            _db.OrderCases.Add(new OrderCase
            {
                MarketItemId = deal.MarketItemId,
                CreatedById = uid,
                Title = $"Спор по сделке #{deal.Id}",
                Text = "Открыт спор по сделке. Средства остаются в холде до решения юриста или администрации.",
                Status = CaseStatuses.Open,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        TempData["DealNotice"] = "Спор открыт. Средства останутся в холде до решения.";
        return RedirectToAction("Index", "Deals");
    }

    [Authorize(Roles = StaffRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TakeCase(int id)
    {
        var item = await _db.OrderCases.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();

        item.Status = CaseStatuses.InProgress;
        item.AgentId = _users.GetUserId(User);
        await _db.SaveChangesAsync();

        TempData["DisputeNotice"] = $"Спор #{item.Id} взят в работу.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = StaffRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetCaseStatus(int id, string status)
    {
        var item = await _db.OrderCases.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();

        item.Status = NormalizeStatus(status);
        item.AgentId = _users.GetUserId(User);
        await _db.SaveChangesAsync();

        TempData["DisputeNotice"] = $"Статус спора #{item.Id} обновлен.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = StaffRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolve(int dealId, string decision)
    {
        var lawyerId = _users.GetUserId(User);
        var deal = await _db.Deals
            .Include(x => x.MarketItem)
            .FirstOrDefaultAsync(x => x.Id == dealId);

        if (deal is null) return NotFound();
        if (deal.Status != DealStatuses.Dispute) return BadRequest();
        if (decision is not ("release" or "refund")) return BadRequest();

        var customerWallet = await GetWallet(deal.CustomerId);
        if (customerWallet.HoldBalance < deal.Amount)
        {
            TempData["DisputeNotice"] = "Нельзя решить спор: в холде заказчика недостаточно средств.";
            return RedirectToAction(nameof(Index));
        }

        await using var tx = await _db.Database.BeginTransactionAsync();
        customerWallet.HoldBalance -= deal.Amount;

        if (decision == "release")
        {
            var executorWallet = await GetWallet(deal.ExecutorId);
            executorWallet.Balance += deal.ExecutorAmount;
            deal.Status = DealStatuses.Completed;
            deal.CompletedAt = DateTime.UtcNow;
            if (deal.MarketItem is not null) deal.MarketItem.OrderStatus = OrderStatuses.Done;

            _db.PaymentTransactions.Add(new PaymentTransaction { UserId = deal.ExecutorId, Amount = deal.ExecutorAmount, Type = PaymentTypes.Release, Status = PaymentStatuses.Success, Comment = $"Решение спора по сделке #{deal.Id}" });
            _db.PaymentTransactions.Add(new PaymentTransaction { UserId = deal.CustomerId, Amount = deal.CommissionAmount, Type = PaymentTypes.Commission, Status = PaymentStatuses.Success, Comment = $"Комиссия по спорной сделке #{deal.Id}" });
            _db.PlatformTransactions.Add(new PlatformTransaction { Amount = deal.CommissionAmount, Type = PlatformTransactionTypes.Commission, DealId = deal.Id });
        }
        else
        {
            customerWallet.Balance += deal.Amount;
            deal.Status = DealStatuses.Refunded;
            deal.CompletedAt = DateTime.UtcNow;
            if (deal.MarketItem is not null) deal.MarketItem.OrderStatus = OrderStatuses.Cancelled;

            _db.PaymentTransactions.Add(new PaymentTransaction { UserId = deal.CustomerId, Amount = deal.Amount, Type = PaymentTypes.Refund, Status = PaymentStatuses.Success, Comment = $"Возврат по спору сделки #{deal.Id}" });
        }

        if (deal.MarketItemId is not null)
        {
            var cases = await _db.OrderCases
                .Where(x => x.MarketItemId == deal.MarketItemId && x.Status != CaseStatuses.Done)
                .ToListAsync();

            foreach (var item in cases)
            {
                item.Status = CaseStatuses.Done;
                item.AgentId = lawyerId;
            }
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        TempData["DisputeNotice"] = decision == "release"
            ? $"Спор по сделке #{deal.Id} решен в пользу исполнителя."
            : $"Спор по сделке #{deal.Id} решен возвратом заказчику.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<Wallet> GetWallet(string userId)
    {
        var wallet = await _db.Wallets.FirstOrDefaultAsync(x => x.UserId == userId);
        if (wallet is not null) return wallet;
        wallet = new Wallet { UserId = userId };
        _db.Wallets.Add(wallet);
        await _db.SaveChangesAsync();
        return wallet;
    }

    private static string NormalizeStatus(string? status)
    {
        return status is CaseStatuses.Open or CaseStatuses.InProgress or CaseStatuses.Done
            ? status
            : CaseStatuses.Open;
    }
}
