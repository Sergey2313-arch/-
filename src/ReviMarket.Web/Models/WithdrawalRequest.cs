using System.ComponentModel.DataAnnotations;

namespace ReviMarket.Web.Models;

public class WithdrawalRequest
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public decimal Amount { get; set; }

    [Required, MaxLength(30)]
    public string Status { get; set; } = WithdrawalStatuses.Pending;

    [MaxLength(300)]
    public string? PaymentInfo { get; set; }

    [MaxLength(300)]
    public string? AdminComment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}

public static class WithdrawalStatuses
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Paid = "Paid";
}
