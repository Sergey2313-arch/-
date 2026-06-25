using System.ComponentModel.DataAnnotations;

namespace ReviMarket.Web.Models;

public class UserReview
{
    public int Id { get; set; }

    [Required]
    public string AuthorId { get; set; } = string.Empty;
    public ApplicationUser? Author { get; set; }

    [Required]
    public string TargetUserId { get; set; } = string.Empty;
    public ApplicationUser? TargetUser { get; set; }

    public int? DealId { get; set; }
    public Deal? Deal { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required, MaxLength(600)]
    public string Text { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string TargetRole { get; set; } = UserRoles.Creator;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
