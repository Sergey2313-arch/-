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
        var creators = await _db.Users
            .Where(x => x.AccountType == UserRoles.Creator || x.AccountType == UserRoles.Admin)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return View(creators);
    }
}
