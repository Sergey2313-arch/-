using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

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
        ViewBag.Deals = await _db.Deals.Include(x => x.Customer).Include(x => x.Executor).Include(x => x.MarketItem).Where(x => x.CustomerId == userId || x.ExecutorId == userId).OrderByDescending(x => x.CreatedAt).Take(8).ToListAsync();
        ViewBag.Withdrawals = await _db.WithdrawalRequests.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).Take(5).ToListAsync();
        ViewBag.EarnedTotal = await _db.Deals.Where(x => x.ExecutorId == userId && x.Status == DealStatuses.Completed).Select(x => (decimal?)x.ExecutorAmount).SumAsync() ?? 0m;
        ViewBag.SpentTotal = await _db.Deals.Where(x => x.CustomerId == userId && x.Status == DealStatuses.Completed).Select(x => (decimal?)x.Amount).SumAsync() ?? 0m;

        return View();
    }
}
