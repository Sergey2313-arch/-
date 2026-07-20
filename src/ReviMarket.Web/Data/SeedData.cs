using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var environment = services.GetRequiredService<IHostEnvironment>();
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

        if (environment.IsDevelopment())
        {
            await SeedDevelopmentCreatorsAsync(userManager, db);
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

    private static async Task SeedDevelopmentCreatorsAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
    {
        var creatorRoleId = await db.Roles
            .Where(x => x.Name == UserRoles.Creator)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        var hasCreators = await db.Users.AnyAsync(x => x.AccountType == UserRoles.Creator)
            || (!string.IsNullOrWhiteSpace(creatorRoleId) && await db.UserRoles.AnyAsync(x => x.RoleId == creatorRoleId));

        if (hasCreators)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var creators = new[]
        {
            new ApplicationUser
            {
                UserName = "creator.design@revimarket.local",
                Email = "creator.design@revimarket.local",
                EmailConfirmed = true,
                DisplayName = "Studio Aurora",
                AccountType = UserRoles.Creator,
                LegalType = AccountLegalTypes.Business,
                OrganizationName = "Studio Aurora",
                Inn = "7700000000",
                OgrnOrOgrnip = "1027700000000",
                LegalAddress = "Москва, ул. Тверская, 7",
                Rating = 4.92m,
                ReviewsCount = 38,
                CreatedAt = now.AddMonths(-9),
                LastSeenAt = now.AddMinutes(-2)
            },
            new ApplicationUser
            {
                UserName = "creator.dev@revimarket.local",
                Email = "creator.dev@revimarket.local",
                EmailConfirmed = true,
                DisplayName = "Nikita Fullstack",
                AccountType = UserRoles.Creator,
                LegalType = AccountLegalTypes.Individual,
                Rating = 4.87m,
                ReviewsCount = 27,
                CreatedAt = now.AddMonths(-7),
                LastSeenAt = now.AddMinutes(-18)
            },
            new ApplicationUser
            {
                UserName = "creator.seo@revimarket.local",
                Email = "creator.seo@revimarket.local",
                EmailConfirmed = true,
                DisplayName = "SEO Lab",
                AccountType = UserRoles.Creator,
                LegalType = AccountLegalTypes.Business,
                OrganizationName = "SEO Lab",
                Inn = "7800000000",
                OgrnOrOgrnip = "1037800000000",
                LegalAddress = "Санкт-Петербург, Невский проспект, 21",
                Rating = 4.75m,
                ReviewsCount = 19,
                CreatedAt = now.AddMonths(-5),
                LastSeenAt = now.AddHours(-3)
            }
        };

        foreach (var creator in creators)
        {
            if (await userManager.FindByEmailAsync(creator.Email!) is not null)
            {
                continue;
            }

            var result = await userManager.CreateAsync(creator);
            if (result.Succeeded && !await userManager.IsInRoleAsync(creator, UserRoles.Creator))
            {
                await userManager.AddToRoleAsync(creator, UserRoles.Creator);
            }
        }
    }
}
