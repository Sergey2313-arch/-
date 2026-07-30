using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;
using ReviMarket.Web.ViewModels;

namespace ReviMarket.Web.Tests;

public class DataIntegrityTests
{
    [Fact]
    public async Task Wallet_user_id_is_unique()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        db.Users.Add(CreateUser("customer"));
        await db.SaveChangesAsync();

        db.Wallets.Add(new Wallet { UserId = "customer" });
        await db.SaveChangesAsync();

        db.Wallets.Add(new Wallet { UserId = "customer" });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Market_item_can_have_only_one_deal()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        db.Users.AddRange(CreateUser("customer"), CreateUser("executor", UserRoles.Creator));
        var item = new MarketItem
        {
            Title = "Test order",
            Description = "Test description",
            Price = 1000,
            Category = MarketCategories.It,
            Type = MarketItemTypes.Order,
            ReviewStatus = ReviewStatuses.Approved,
            OrderStatus = OrderStatuses.InWork,
            OwnerId = "customer",
            AssignedExecutorId = "executor"
        };
        db.MarketItems.Add(item);
        await db.SaveChangesAsync();

        db.Deals.Add(CreateDeal(item.Id));
        await db.SaveChangesAsync();

        db.Deals.Add(CreateDeal(item.Id));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Provider_payment_id_is_unique()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        db.Users.Add(CreateUser("customer"));
        await db.SaveChangesAsync();

        db.PaymentInvoices.Add(new PaymentInvoice
        {
            UserId = "customer",
            Amount = 100,
            ProviderPaymentId = "provider-1"
        });
        await db.SaveChangesAsync();

        db.PaymentInvoices.Add(new PaymentInvoice
        {
            UserId = "customer",
            Amount = 200,
            ProviderPaymentId = "provider-1"
        });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public void Create_order_model_rejects_invalid_budget()
    {
        var model = new CreateOrderViewModel
        {
            Title = "Test order",
            Description = "Test description",
            Price = 99,
            Category = MarketCategories.It
        };
        var results = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(model, new ValidationContext(model), results, true);

        Assert.False(valid);
        Assert.Contains(results, x => x.MemberNames.Contains(nameof(CreateOrderViewModel.Price)));
    }

    private static ApplicationDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        return new ApplicationDbContext(options);
    }

    private static ApplicationUser CreateUser(string id, string role = UserRoles.Customer) => new()
    {
        Id = id,
        UserName = $"{id}@example.test",
        NormalizedUserName = $"{id.ToUpperInvariant()}@EXAMPLE.TEST",
        Email = $"{id}@example.test",
        NormalizedEmail = $"{id.ToUpperInvariant()}@EXAMPLE.TEST",
        DisplayName = id,
        AccountType = role,
        SecurityStamp = Guid.NewGuid().ToString("N")
    };

    private static Deal CreateDeal(int marketItemId) => new()
    {
        CustomerId = "customer",
        ExecutorId = "executor",
        MarketItemId = marketItemId,
        Amount = 1000,
        CommissionAmount = 100,
        ExecutorAmount = 900,
        Status = DealStatuses.Funded
    };
}
