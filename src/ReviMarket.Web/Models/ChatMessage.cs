using System.ComponentModel.DataAnnotations;

namespace ReviMarket.Web.Models;

public class ChatMessage
{
    public int Id { get; set; }

    [Required]
    public string SenderId { get; set; } = string.Empty;
    public ApplicationUser? Sender { get; set; }

    [Required]
    public string ReceiverId { get; set; } = string.Empty;
    public ApplicationUser? Receiver { get; set; }

    [Required, MaxLength(1000)]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
