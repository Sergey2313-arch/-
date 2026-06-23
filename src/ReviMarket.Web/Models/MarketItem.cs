using System.ComponentModel.DataAnnotations;

namespace ReviMarket.Web.Models;

public class MarketItem
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(600)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 10_000_000)]
    public decimal Price { get; set; }

    [Required, MaxLength(40)]
    public string Category { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Type { get; set; } = MarketItemTypes.Product;

    [MaxLength(300)]
    public string? ImagePath { get; set; }

    [Required, MaxLength(20)]
    public string ReviewStatus { get; set; } = ReviewStatuses.Pending;

    public string? OwnerId { get; set; }
    public ApplicationUser? Owner { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public static class MarketItemTypes
{
    public const string Product = "Product";
    public const string Order = "Order";
    public const string Vacancy = "Vacancy";
}

public static class ReviewStatuses
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Blocked = "Blocked";
}
