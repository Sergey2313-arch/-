using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentWebhooksController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public PaymentWebhooksController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpPost("test-success/{invoiceId:int}")]
    public async Task<IActionResult> TestSuccess(int invoiceId)
    {
        var invoice = await _db.PaymentInvoices.FirstOrDefaultAsync(x => x.Id == invoiceId);
        if (invoice is null) return NotFound();
        if (invoice.Status == PaymentStatuses.Success) return Ok();

        var wallet = await _db.Wallets.FirstOrDefaultAsync(x => x.UserId == invoice.UserId);
        if (wallet is null)
        {
            wallet = new Wallet { UserId = invoice.UserId };
            _db.Wallets.Add(wallet);
        }

        invoice.Status = PaymentStatuses.Success;
        invoice.PaidAt = DateTime.UtcNow;
        wallet.Balance += invoice.Amount;

        _db.PaymentTransactions.Add(new PaymentTransaction
        {
            UserId = invoice.UserId,
            Amount = invoice.Amount,
            Type = PaymentTypes.TopUp,
            Status = PaymentStatuses.Success,
            Comment = "Payment invoice success"
        });

        await _db.SaveChangesAsync();
        return Ok();
    }
}
