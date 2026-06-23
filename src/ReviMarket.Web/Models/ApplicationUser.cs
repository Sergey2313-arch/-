using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ReviMarket.Web.Models;

public class ApplicationUser : IdentityUser
{
    [Required, MaxLength(60)]
    public string DisplayName { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string AccountType { get; set; } = UserRoles.Customer;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
