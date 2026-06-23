using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Controllers;

public class CatalogController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public CatalogController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
    {
        _db = db;
        _userManager = userManager;
        _environment = environment;
    }

    public async Task<IActionResult> Index(string? search, string? category)
    {
        var query = _db.MarketItems
            .Include(x => x.Owner)
            .Where(x => x.Type == MarketItemTypes.Product && x.ReviewStatus == ReviewStatuses.Approved);

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(x => x.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Title.Contains(search) || x.Description.Contains(search));
        }

        ViewBag.Search = search;
        ViewBag.Category = category;
        ViewBag.Categories = MarketCategories.All;
        return View(await query.OrderByDescending(x => x.CreatedAt).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _db.MarketItems.Include(x => x.Owner).FirstOrDefaultAsync(x => x.Id == id && x.Type == MarketItemTypes.Product);
        if (item is null) return NotFound();
        return View(item);
    }

    [Authorize(Roles = UserRoles.Creator + "," + UserRoles.Admin)]
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Categories = MarketCategories.All;
        return View(new MarketItem { Type = MarketItemTypes.Product, Category = MarketCategories.Design });
    }

    [Authorize(Roles = UserRoles.Creator + "," + UserRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MarketItem item, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = MarketCategories.All;
            return View(item);
        }

        item.Type = MarketItemTypes.Product;
        item.OwnerId = _userManager.GetUserId(User);
        item.CreatedAt = DateTime.UtcNow;
        item.ReviewStatus = User.IsInRole(UserRoles.Admin) ? ReviewStatuses.Approved : ReviewStatuses.Pending;

        if (imageFile is not null && imageFile.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(string.Empty, "Можно загрузить только JPG, PNG или WEBP.");
                ViewBag.Categories = MarketCategories.All;
                return View(item);
            }

            if (imageFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(string.Empty, "Изображение не должно быть больше 5 МБ.");
                ViewBag.Categories = MarketCategories.All;
                return View(item);
            }

            var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            await using var stream = System.IO.File.Create(filePath);
            await imageFile.CopyToAsync(stream);

            item.ImagePath = $"/uploads/products/{fileName}";
        }

        _db.MarketItems.Add(item);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
