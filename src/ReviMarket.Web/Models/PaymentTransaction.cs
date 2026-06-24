using System.ComponentModel.DataAnnotations;

namespace ReviMarket.Web.Models;

public class PaymentTransaction
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public decimal Amount { get; set; }

    [Required, MaxLength(30)]
    public string Type { get; set; } = PaymentTypes.TopUp;

    [Required, MaxLength(30)]
    public string Status { get; set; } = PaymentStatuses.Success;

    [MaxLength(300)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public static class PaymentTypes
{
    public const string TopUp = "TopUp";
    public const string Hold = "Hold";
    public const string Release = "Release";
    public const string Commission = "Commission";
    public const string Refund = "Refund";
    public const string Withdraw = "Withdraw";
}

public static class PaymentStatuses
{
    public const string Pending = "Pending";
    public const string Success = "Success";
    public const string Failed = "Failed";
}
