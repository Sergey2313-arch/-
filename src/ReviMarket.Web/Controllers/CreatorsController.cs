using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Controllers;

public class CreatorsController : Controller
{
    private readonly ApplicationDbContext _db;

    public CreatorsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var creatorRoleId = await _db.Roles
            .Where(x => x.Name == UserRoles.Creator)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        var creatorRoleUserIds = string.IsNullOrWhiteSpace(creatorRoleId)
            ? new HashSet<string>()
            : (await _db.UserRoles
                .Where(x => x.RoleId == creatorRoleId)
                .Select(x => x.UserId)
                .ToListAsync())
                .ToHashSet();

        var creators = (await _db.Users
            .AsNoTracking()
            .ToListAsync())
            .Where(x => x.AccountType == UserRoles.Creator || creatorRoleUserIds.Contains(x.Id))
            .OrderByDescending(x => x.Rating)
            .ThenByDescending(x => x.ReviewsCount)
            .ThenByDescending(x => x.LastSeenAt)
            .ToList();

        ViewBag.OnlineLimit = DateTime.UtcNow.AddMinutes(-5);
        return View(creators);
    }
}
