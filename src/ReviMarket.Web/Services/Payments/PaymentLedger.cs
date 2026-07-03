using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Services.Payments;

public sealed class PaymentLedger
{
    private readonly ApplicationDbContext _db;

    public PaymentLedger(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<bool> MarkInvoicePaidAsync(PaymentInvoice invoice, string comment, CancellationToken cancellationToken)
    {
        if (invoice.Status == PaymentStatuses.Success)
        {
            return false;
        }

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        var wallet = await _db.Wallets.FirstOrDefaultAsync(x => x.UserId == invoice.UserId, cancellationToken);
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
            Comment = comment
        });

        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return true;
    }

    public async Task MarkInvoiceFailedAsync(PaymentInvoice invoice, CancellationToken cancellationToken)
    {
        if (invoice.Status == PaymentStatuses.Success || invoice.Status == PaymentStatuses.Failed)
        {
            return;
        }

        invoice.Status = PaymentStatuses.Failed;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
