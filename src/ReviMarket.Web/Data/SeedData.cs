using Microsoft.AspNetCore.Identity;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var db = services.GetRequiredService<ApplicationDbContext>();

        foreach (var role in UserRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminEmail = configuration["Admin:BootstrapEmail"];
        if (!string.IsNullOrWhiteSpace(adminEmail))
        {
            var adminUser = await userManager.FindByEmailAsync(adminEmail.Trim());
            if (adminUser is not null && !await userManager.IsInRoleAsync(adminUser, UserRoles.Admin))
            {
                adminUser.AccountType = UserRoles.Admin;
                await userManager.UpdateAsync(adminUser);
                await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
            }
        }

        if (!db.MarketItems.Any(x => x.Type == MarketItemTypes.Order))
        {
            db.MarketItems.AddRange(
                new MarketItem { Type = MarketItemTypes.Order, Title = "Карточки товара для WB/Ozon", Description = "Нужно 8 карточек в черно-фиолетовом стиле: преимущества, инфографика, чистая подача.", Price = 7500, Category = MarketCategories.Design, ReviewStatus = ReviewStatuses.Approved, OrderStatus = OrderStatuses.Open },
                new MarketItem { Type = MarketItemTypes.Order, Title = "Сделать лендинг услуги", Description = "Главный экран, преимущества, цены, отзывы и форма заявки.", Price = 15000, Category = MarketCategories.It, ReviewStatus = ReviewStatuses.Approved, OrderStatus = OrderStatuses.Open },
                new MarketItem { Type = MarketItemTypes.Order, Title = "SEO-тексты для сайта", Description = "Нужны тексты под ключевые запросы, структура страниц и meta-описания.", Price = 6000, Category = MarketCategories.Seo, ReviewStatus = ReviewStatuses.Approved, OrderStatus = OrderStatuses.Open }
            );
            await db.SaveChangesAsync();
        }
    }
}
