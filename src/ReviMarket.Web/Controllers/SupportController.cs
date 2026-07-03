using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;
using ReviMarket.Web.Services.Security;
using ReviMarket.Web.ViewModels;

namespace ReviMarket.Web.Controllers;

[Authorize]
public class SupportController : Controller
{
    private static readonly string[] StaffRoles =
    {
        UserRoles.Admin,
        UserRoles.Manager,
        UserRoles.Owner,
        UserRoles.CoOwner,
        UserRoles.SupportAgent
    };

    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly TextSecurityService _textSecurity;

    public SupportController(ApplicationDbContext db, UserManager<ApplicationUser> users, TextSecurityService textSecurity)
    {
        _db = db;
        _users = users;
        _textSecurity = textSecurity;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _users.GetUserId(User)!;
        var requests = await _db.SupportRequests
            .Include(x => x.Agent)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return View(new SupportIndexViewModel { Requests = requests });
    }

    [HttpGet]
    public IActionResult Create() => View(new SupportRequest());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupportRequest request)
    {
        var title = _textSecurity.CleanText(request.Title, 120);
        var text = _textSecurity.CleanText(request.Text, 1200);

        if (title.Length < 3) ModelState.AddModelError(nameof(request.Title), "Коротко опиши тему обращения.");
        if (text.Length < 10) ModelState.AddModelError(nameof(request.Text), "Опиши проблему подробнее.");
        if (_textSecurity.LooksDangerous(request.Title) || _textSecurity.LooksDangerous(request.Text))
        {
            ModelState.AddModelError(string.Empty, "Обращение похоже на вредоносный ввод и не отправлено.");
        }

        if (!ModelState.IsValid)
        {
            request.Title = title;
            request.Text = text;
            return View(request);
        }

        request.Title = title;
        request.Text = text;
        request.UserId = _users.GetUserId(User);
        request.Status = CaseStatuses.Open;
        request.Priority = NormalizePriority(request.Priority);
        request.CreatedAt = DateTime.UtcNow;

        _db.SupportRequests.Add(request);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = request.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var request = await _db.SupportRequests
            .Include(x => x.User)
            .Include(x => x.Agent)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (request is null) return NotFound();
        if (!CanSee(request)) return NotFound();

        return View(new SupportDetailsViewModel { Request = request });
    }

    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Manager + "," + UserRoles.Owner + "," + UserRoles.CoOwner + "," + UserRoles.SupportAgent)]
    [HttpGet]
    public async Task<IActionResult> Queue()
    {
        var requests = await _db.SupportRequests
            .Include(x => x.User)
            .Include(x => x.Agent)
            .OrderBy(x => x.Status == CaseStatuses.Open ? 0 : x.Status == CaseStatuses.InProgress ? 1 : 2)
            .ThenByDescending(x => x.CreatedAt)
            .Take(100)
            .ToListAsync();

        return View(new SupportQueueViewModel { Requests = requests });
    }

    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Manager + "," + UserRoles.Owner + "," + UserRoles.CoOwner + "," + UserRoles.SupportAgent)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Take(int id)
    {
        var request = await _db.SupportRequests.FirstOrDefaultAsync(x => x.Id == id);
        if (request is null) return NotFound();

        request.AgentId = _users.GetUserId(User);
        request.Status = CaseStatuses.InProgress;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Manager + "," + UserRoles.Owner + "," + UserRoles.CoOwner + "," + UserRoles.SupportAgent)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, string status)
    {
        var request = await _db.SupportRequests.FirstOrDefaultAsync(x => x.Id == id);
        if (request is null) return NotFound();

        request.Status = NormalizeStatus(status);
        if (string.IsNullOrWhiteSpace(request.AgentId))
        {
            request.AgentId = _users.GetUserId(User);
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    private bool CanSee(SupportRequest request)
    {
        var userId = _users.GetUserId(User);
        return request.UserId == userId || StaffRoles.Any(User.IsInRole);
    }

    private static string NormalizePriority(string? priority)
    {
        return priority is "Low" or "High" or "Urgent" ? priority : "Normal";
    }

    private static string NormalizeStatus(string? status)
    {
        return status is CaseStatuses.Open or CaseStatuses.InProgress or CaseStatuses.Done
            ? status
            : CaseStatuses.Open;
    }
}
