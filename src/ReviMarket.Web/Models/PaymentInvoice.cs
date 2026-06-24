using System.ComponentModel.DataAnnotations;

namespace ReviMarket.Web.Models;

public class PaymentInvoice
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public decimal Amount { get; set; }

    [Required, MaxLength(40)]
    public string Provider { get; set; } = PaymentProviders.Test;

    [MaxLength(120)]
    public string? ProviderPaymentId { get; set; }

    [Required, MaxLength(30)]
    public string Status { get; set; } = PaymentStatuses.Pending;

    [MaxLength(500)]
    public string? ConfirmationUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PaidAt { get; set; }
}

public static class PaymentProviders
{
    public const string Test = "Test";
    public const string YooKassa = "YooKassa";
    public const string TBank = "TBank";
}
