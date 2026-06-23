using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Controllers;

[Authorize(Roles = UserRoles.Admin + "," + UserRoles.Manager)]
public class TeamController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public TeamController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.OrderByDescending(x => x.CreatedAt).ToListAsync();
        return View(users);
    }
}
