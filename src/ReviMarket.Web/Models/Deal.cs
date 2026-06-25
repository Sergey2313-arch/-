using System.ComponentModel.DataAnnotations;

namespace ReviMarket.Web.Models;

public class Deal
{
    public int Id { get; set; }

    [Required]
    public string CustomerId { get; set; } = string.Empty;
    public ApplicationUser? Customer { get; set; }

    [Required]
    public string ExecutorId { get; set; } = string.Empty;
    public ApplicationUser? Executor { get; set; }

    public int? MarketItemId { get; set; }
    public MarketItem? MarketItem { get; set; }

    public decimal Amount { get; set; }
    public decimal CommissionPercent { get; set; } = 10;
    public decimal CommissionAmount { get; set; }
    public decimal ExecutorAmount { get; set; }

    [Required, MaxLength(30)]
    public string Status { get; set; } = DealStatuses.Funded;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

public static class DealStatuses
{
    public const string Created = "Created";
    public const string Funded = "Funded";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Dispute = "Dispute";
    public const string Cancelled = "Cancelled";
    public const string Refunded = "Refunded";
}
