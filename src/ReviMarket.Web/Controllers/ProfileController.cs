using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;
using ReviMarket.Web.ViewModels;

namespace ReviMarket.Web.Controllers;

public class ProfileController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            ViewBag.User = null;
            return View(new List<MarketItem>());
        }

        var userId = _userManager.GetUserId(User)!;
        var user = await _userManager.GetUserAsync(User);
        var wallet = await _db.Wallets.FirstOrDefaultAsync(x => x.UserId == userId);
        if (wallet is null)
        {
            wallet = new Wallet { UserId = userId };
            _db.Wallets.Add(wallet);
            await _db.SaveChangesAsync();
        }

        ViewBag.User = user;
        ViewBag.Wallet = wallet;
        ViewBag.CreatedOrders = await _db.MarketItems.Include(x => x.AssignedExecutor).Where(x => x.Type == MarketItemTypes.Order && x.OwnerId == userId).OrderByDescending(x => x.CreatedAt).ToListAsync();
        ViewBag.TakenOrders = await _db.MarketItems.Include(x => x.Owner).Where(x => x.Type == MarketItemTypes.Order && x.AssignedExecutorId == userId).OrderByDescending(x => x.AssignedAt).ToListAsync();
        ViewBag.Withdrawals = await _db.WithdrawalRequests.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).Take(5).ToListAsync();
        var earnedAmounts = await _db.Deals
            .Where(x => x.ExecutorId == userId && x.Status == DealStatuses.Completed)
            .Select(x => x.ExecutorAmount)
            .ToListAsync();
        var spentAmounts = await _db.Deals
            .Where(x => x.CustomerId == userId && x.Status == DealStatuses.Completed)
            .Select(x => x.Amount)
            .ToListAsync();

        ViewBag.EarnedTotal = earnedAmounts.Sum();
        ViewBag.SpentTotal = spentAmounts.Sum();

        return View();
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return NotFound();

        return View(new ProfileEditViewModel
        {
            DisplayName = user.DisplayName,
            AccountType = AccountController.NormalizeAccountType(user.AccountType),
            LegalType = AccountController.NormalizeLegalType(user.LegalType),
            OrganizationName = user.OrganizationName,
            Inn = user.Inn,
            OgrnOrOgrnip = user.OgrnOrOgrnip,
            LegalAddress = user.LegalAddress
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return NotFound();

        var role = AccountController.NormalizeAccountType(model.AccountType);
        var legalType = AccountController.NormalizeLegalType(model.LegalType);

        user.DisplayName = model.DisplayName.Trim();
        user.AccountType = role;
        user.LegalType = legalType;
        user.OrganizationName = legalType == AccountLegalTypes.Business ? model.OrganizationName?.Trim() : null;
        user.Inn = legalType == AccountLegalTypes.Business ? AccountController.NormalizeDigits(model.Inn) : null;
        user.OgrnOrOgrnip = legalType == AccountLegalTypes.Business ? AccountController.NormalizeDigits(model.OgrnOrOgrnip) : null;
        user.LegalAddress = legalType == AccountLegalTypes.Business ? model.LegalAddress?.Trim() : null;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await SyncMarketplaceRoleAsync(user, role);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Public(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();
        ViewBag.Reviews = await _db.UserReviews.Include(x => x.Author).Include(x => x.Deal).ThenInclude(x => x!.MarketItem).Where(x => x.TargetUserId == id).OrderByDescending(x => x.CreatedAt).Take(20).ToListAsync();
        ViewBag.IsOnline = user.LastSeenAt is not null && user.LastSeenAt > DateTime.UtcNow.AddMinutes(-5);
        return View(user);
    }

    private async Task SyncMarketplaceRoleAsync(ApplicationUser user, string role)
    {
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var marketplaceRole in new[] { UserRoles.Customer, UserRoles.Creator })
        {
            if (roles.Contains(marketplaceRole) && marketplaceRole != role)
            {
                await _userManager.RemoveFromRoleAsync(user, marketplaceRole);
            }
        }

        if (!await _userManager.IsInRoleAsync(user, role))
        {
            await _userManager.AddToRoleAsync(user, role);
        }
    }
}
