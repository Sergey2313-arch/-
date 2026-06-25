using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ReviMarket.Web.Models;

public class ApplicationUser : IdentityUser
{
    [Required, MaxLength(60)]
    public string DisplayName { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string AccountType { get; set; } = UserRoles.Customer;

    [Required, MaxLength(30)]
    public string LegalType { get; set; } = AccountLegalTypes.Individual;

    [MaxLength(160)]
    public string? OrganizationName { get; set; }

    [MaxLength(20)]
    public string? Inn { get; set; }

    [MaxLength(30)]
    public string? OgrnOrOgrnip { get; set; }

    [MaxLength(250)]
    public string? LegalAddress { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
