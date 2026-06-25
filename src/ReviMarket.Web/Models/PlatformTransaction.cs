using System.ComponentModel.DataAnnotations;

namespace ReviMarket.Web.Models;

public class PlatformTransaction
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    [Required, MaxLength(40)]
    public string Type { get; set; } = PlatformTransactionTypes.Commission;

    [MaxLength(300)]
    public string? Comment { get; set; }

    public int? DealId { get; set; }
    public Deal? Deal { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public static class PlatformTransactionTypes
{
    public const string Commission = "Commission";
    public const string ManualCorrection = "ManualCorrection";
}
