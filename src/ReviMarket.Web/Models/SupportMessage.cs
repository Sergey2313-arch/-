using System.ComponentModel.DataAnnotations;

namespace ReviMarket.Web.Models;

public class SupportMessage
{
    public int Id { get; set; }

    public int SupportRequestId { get; set; }
    public SupportRequest? SupportRequest { get; set; }

    public string? SenderId { get; set; }
    public ApplicationUser? Sender { get; set; }

    [Required, MaxLength(1200)]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
