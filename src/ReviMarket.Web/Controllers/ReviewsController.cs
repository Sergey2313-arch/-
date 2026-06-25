using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Controllers;

[Authorize]
public class ReviewsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public ReviewsController(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int dealId, string targetUserId, int rating, string text)
    {
        var uid = _users.GetUserId(User)!;
        rating = Math.Clamp(rating, 1, 5);
        text = (text ?? string.Empty).Trim();
        if (text.Length < 3) return RedirectToAction("Index", "Deals");

        var deal = await _db.Deals.FirstOrDefaultAsync(x => x.Id == dealId && x.Status == DealStatuses.Completed);
        if (deal is null) return NotFound();

        var isCustomerReviewingExecutor = deal.CustomerId == uid && deal.ExecutorId == targetUserId;
        var isExecutorReviewingCustomer = deal.ExecutorId == uid && deal.CustomerId == targetUserId;
        if (!isCustomerReviewingExecutor && !isExecutorReviewingCustomer) return Forbid();

        var exists = await _db.UserReviews.AnyAsync(x => x.DealId == dealId && x.AuthorId == uid && x.TargetUserId == targetUserId);
        if (exists) return RedirectToAction("Index", "Deals");

        _db.UserReviews.Add(new UserReview
        {
            AuthorId = uid,
            TargetUserId = targetUserId,
            DealId = dealId,
            Rating = rating,
            Text = text.Length > 600 ? text[..600] : text,
            TargetRole = isCustomerReviewingExecutor ? UserRoles.Creator : UserRoles.Customer
        });

        await _db.SaveChangesAsync();
        await RecalculateRating(targetUserId);
        return RedirectToAction("Public", "Profile", new { id = targetUserId });
    }

    private async Task RecalculateRating(string userId)
    {
        var target = await _users.FindByIdAsync(userId);
        if (target is null) return;
        var reviews = await _db.UserReviews.Where(x => x.TargetUserId == userId).ToListAsync();
        target.ReviewsCount = reviews.Count;
        target.Rating = reviews.Count == 0 ? 0 : Math.Round((decimal)reviews.Average(x => x.Rating), 2);
        await _users.UpdateAsync(target);
    }
}
