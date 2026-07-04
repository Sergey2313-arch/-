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
        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        var pendingInvoice = await _db.PaymentInvoices
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == invoice.Id, cancellationToken);

        if (pendingInvoice is null || pendingInvoice.Status != PaymentStatuses.Pending)
        {
            await tx.CommitAsync(cancellationToken);
            return false;
        }

        var updated = await _db.PaymentInvoices
            .Where(x => x.Id == invoice.Id && x.Status == PaymentStatuses.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, PaymentStatuses.Success)
                .SetProperty(x => x.PaidAt, DateTime.UtcNow), cancellationToken);

        if (updated == 0)
        {
            await tx.CommitAsync(cancellationToken);
            return false;
        }

        var wallet = await _db.Wallets.FirstOrDefaultAsync(x => x.UserId == pendingInvoice.UserId, cancellationToken);
        if (wallet is null)
        {
            wallet = new Wallet { UserId = pendingInvoice.UserId };
            _db.Wallets.Add(wallet);
        }

        wallet.Balance += pendingInvoice.Amount;

        _db.PaymentTransactions.Add(new PaymentTransaction
        {
            UserId = pendingInvoice.UserId,
            Amount = pendingInvoice.Amount,
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
        await _db.PaymentInvoices
            .Where(x => x.Id == invoice.Id && x.Status == PaymentStatuses.Pending)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Status, PaymentStatuses.Failed), cancellationToken);
    }
}
