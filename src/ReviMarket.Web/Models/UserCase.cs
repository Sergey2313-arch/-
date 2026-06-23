using System.ComponentModel.DataAnnotations;

namespace ReviMarket.Web.Models;

public class UserCase
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(1200)]
    public string Text { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Status { get; set; } = CaseStatuses.Open;

    public string? CreatedById { get; set; }
    public ApplicationUser? CreatedBy { get; set; }

    public string? TargetUserId { get; set; }
    public ApplicationUser? TargetUser { get; set; }

    public string? AgentId { get; set; }
    public ApplicationUser? Agent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
