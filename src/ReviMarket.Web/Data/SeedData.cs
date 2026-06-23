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

        foreach (var role in new[] { UserRoles.Customer, UserRoles.Creator, UserRoles.Admin })
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

        if (!db.MarketItems.Any())
        {
            db.MarketItems.AddRange(
                new MarketItem { Type = MarketItemTypes.Product, Title = "Figma-шаблоны карточек", Description = "Готовые шаблоны карточек WB/Ozon для быстрого старта.", Price = 1990, Category = MarketCategories.Design },
                new MarketItem { Type = MarketItemTypes.Product, Title = "Аудит карточки товара", Description = "Разбор визуала, оффера и ошибок карточки товара.", Price = 2500, Category = MarketCategories.Marketplaces },
                new MarketItem { Type = MarketItemTypes.Product, Title = "Пакет AI-промптов для карточек", Description = "Промпты для генерации описаний, преимуществ и идей визуала.", Price = 1490, Category = MarketCategories.Ai },
                new MarketItem { Type = MarketItemTypes.Order, Title = "Карточки товара для WB/Ozon", Description = "Нужно 8 карточек в черно-фиолетовом стиле: преимущества, инфографика, чистая подача.", Price = 7500, Category = MarketCategories.Design },
                new MarketItem { Type = MarketItemTypes.Order, Title = "Сделать лендинг услуги", Description = "Главный экран, преимущества, цены, отзывы и форма заявки.", Price = 15000, Category = MarketCategories.It },
                new MarketItem { Type = MarketItemTypes.Order, Title = "SEO-тексты для сайта", Description = "Нужны тексты под ключевые запросы, структура страниц и meta-описания.", Price = 6000, Category = MarketCategories.Seo }
            );
            await db.SaveChangesAsync();
        }
    }
}
