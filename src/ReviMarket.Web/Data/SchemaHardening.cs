using Microsoft.EntityFrameworkCore;

namespace ReviMarket.Web.Data;

public static class SchemaHardening
{
    public static async Task ApplyAsync(ApplicationDbContext db)
    {
        if (!db.Database.IsSqlite()) return;

        var commands = new[]
        {
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Wallets_UserId\" ON \"Wallets\" (\"UserId\");",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Deals_MarketItemId\" ON \"Deals\" (\"MarketItemId\") WHERE \"MarketItemId\" IS NOT NULL;",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_PaymentInvoices_ProviderPaymentId\" ON \"PaymentInvoices\" (\"ProviderPaymentId\") WHERE \"ProviderPaymentId\" IS NOT NULL;",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_PlatformTransactions_DealId\" ON \"PlatformTransactions\" (\"DealId\") WHERE \"DealId\" IS NOT NULL;"
        };

        foreach (var command in commands)
        {
            await db.Database.ExecuteSqlRawAsync(command);
        }
    }
}
