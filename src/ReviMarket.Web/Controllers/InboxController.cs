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
public class InboxController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TextSecurityService _textSecurity;

    public InboxController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        TextSecurityService textSecurity)
    {
        _db = db;
        _userManager = userManager;
        _textSecurity = textSecurity;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var messages = await _db.ChatMessages
            .Include(x => x.Sender)
            .Include(x => x.Receiver)
            .Where(x => x.SenderId == userId || x.ReceiverId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var conversations = messages
            .GroupBy(x => x.SenderId == userId ? x.ReceiverId : x.SenderId)
            .Select(group =>
            {
                var latest = group.OrderByDescending(x => x.CreatedAt).First();
                var user = latest.SenderId == userId ? latest.Receiver : latest.Sender;
                return user is null
                    ? null
                    : new ConversationViewModel
                    {
                        User = user,
                        LatestMessage = latest,
                        MessagesCount = group.Count()
                    };
            })
            .Where(x => x is not null)
            .Cast<ConversationViewModel>()
            .OrderByDescending(x => x.LatestMessage.CreatedAt)
            .ToList();

        var contactPool = await _userManager.Users
            .Where(x => x.Id != userId)
            .OrderByDescending(x => x.LastSeenAt)
            .ThenBy(x => x.DisplayName)
            .Take(100)
            .ToListAsync();
        var now = DateTimeOffset.UtcNow;
        var contacts = contactPool
            .Where(x => x.LockoutEnd is null || x.LockoutEnd <= now)
            .Take(50)
            .ToList();

        return View(new InboxIndexViewModel
        {
            Conversations = conversations,
            Contacts = contacts
        });
    }

    [HttpGet]
    public async Task<IActionResult> Thread(string id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null) return Challenge();

        var otherUser = await _userManager.FindByIdAsync(id);
        if (otherUser is null) return NotFound();
        if (otherUser.Id == currentUser.Id) return RedirectToAction(nameof(Index));
        if (otherUser.LockoutEnd is not null && otherUser.LockoutEnd > DateTimeOffset.UtcNow) return NotFound();

        var messages = await _db.ChatMessages
            .Include(x => x.Sender)
            .Include(x => x.Receiver)
            .Where(x =>
                (x.SenderId == currentUser.Id && x.ReceiverId == otherUser.Id)
                || (x.SenderId == otherUser.Id && x.ReceiverId == currentUser.Id))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        return View(new ChatThreadViewModel
        {
            CurrentUser = currentUser,
            OtherUser = otherUser,
            Messages = messages
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string receiverId, string text)
    {
        var senderId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(senderId)) return Challenge();
        if (string.IsNullOrWhiteSpace(receiverId) || receiverId == senderId) return BadRequest();

        var receiver = await _userManager.FindByIdAsync(receiverId);
        if (receiver is null) return NotFound();
        if (receiver.LockoutEnd is not null && receiver.LockoutEnd > DateTimeOffset.UtcNow) return NotFound();

        var cleanText = _textSecurity.CleanText(text, 1000);
        if (cleanText.Length < 1)
        {
            TempData["Error"] = "Сообщение не может быть пустым.";
            return RedirectToAction(nameof(Thread), new { id = receiverId });
        }

        if (_textSecurity.LooksDangerous(text))
        {
            TempData["Error"] = "Сообщение похоже на вредоносный код и не отправлено.";
            return RedirectToAction(nameof(Thread), new { id = receiverId });
        }

        var recentLimit = DateTime.UtcNow.AddMinutes(-1);
        var recentCount = await _db.ChatMessages.CountAsync(x => x.SenderId == senderId && x.CreatedAt >= recentLimit);
        if (recentCount >= 20)
        {
            TempData["Error"] = "Слишком много сообщений за минуту. Подожди немного.";
            return RedirectToAction(nameof(Thread), new { id = receiverId });
        }

        _db.ChatMessages.Add(new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Text = cleanText,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Thread), new { id = receiverId });
    }
}
