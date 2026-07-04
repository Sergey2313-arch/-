using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;
using ReviMarket.Web.Options;
using ReviMarket.Web.Services.Payments;
using ReviMarket.Web.Services.Security;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=revimarket.db";

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys")));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.SlidingExpiration = true;
});

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var key = context.User.Identity?.IsAuthenticated == true
            ? context.User.Identity.Name ?? "user"
            : context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 180,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
});

builder.Services.AddControllersWithViews();
builder.Services.Configure<PaymentOptions>(builder.Configuration.GetSection("Payments"));
builder.Services.PostConfigure<PaymentOptions>(options =>
{
    options.Provider = builder.Configuration["PAYMENT_PROVIDER"] ?? options.Provider;
    options.Currency = builder.Configuration["PAYMENT_CURRENCY"] ?? options.Currency;
    options.YooKassa.ShopId = builder.Configuration["PAYMENT_SHOP_ID"] ?? options.YooKassa.ShopId;
    options.YooKassa.SecretKey = builder.Configuration["PAYMENT_SECRET_KEY"] ?? options.YooKassa.SecretKey;
    options.YooKassa.ApiBaseUrl = builder.Configuration["PAYMENT_YOOKASSA_API_BASE_URL"] ?? options.YooKassa.ApiBaseUrl;
    options.ReturnUrl = builder.Configuration["PAYMENT_RETURN_URL"] ?? options.ReturnUrl;
});
builder.Services.AddSingleton<TestPaymentProvider>();
builder.Services.AddHttpClient<YooKassaPaymentProvider>();
builder.Services.AddScoped<PaymentProviderSelector>();
builder.Services.AddScoped<PaymentLedger>();
builder.Services.AddScoped<PaymentReconciliationService>();
builder.Services.AddSingleton<TextSecurityService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SeedData.InitializeAsync(services);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();
app.UseRateLimiter();

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";
    headers["Content-Security-Policy"] = "default-src 'self'; img-src 'self' data:; style-src 'self' 'unsafe-inline'; script-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";

    await next();
});

app.UseAuthentication();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        using var scope = context.RequestServices.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            context.Response.Cookies.Delete(".AspNetCore.Identity.Application");
            context.Response.Redirect("/Account/Login");
            return;
        }

        if (user.LastSeenAt is null || user.LastSeenAt < DateTime.UtcNow.AddMinutes(-1))
        {
            user.LastSeenAt = DateTime.UtcNow;
            await userManager.UpdateAsync(user);
        }
    }

    await next();
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
