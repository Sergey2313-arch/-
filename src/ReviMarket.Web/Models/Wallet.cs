using System.ComponentModel.DataAnnotations;

namespace ReviMarket.Web.Models;

public class Wallet
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public decimal Balance { get; set; }

    public decimal HoldBalance { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
