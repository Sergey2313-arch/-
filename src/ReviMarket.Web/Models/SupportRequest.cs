using System.ComponentModel.DataAnnotations;

namespace ReviMarket.Web.Models;

public class SupportRequest
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(1200)]
    public string Text { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Status { get; set; } = CaseStatuses.Open;

    [MaxLength(20)]
    public string Priority { get; set; } = "Normal";

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public string? AgentId { get; set; }
    public ApplicationUser? Agent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public static class CaseStatuses
{
    public const string Open = "Open";
    public const string InProgress = "InProgress";
    public const string Done = "Done";
}
