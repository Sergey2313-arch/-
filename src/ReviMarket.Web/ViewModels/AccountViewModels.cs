using System.ComponentModel.DataAnnotations;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Укажи имя")]
    [MaxLength(60)]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажи email")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажи пароль")]
    [MinLength(6, ErrorMessage = "Минимум 6 символов")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string AccountType { get; set; } = UserRoles.Customer;

    [Required]
    public string LegalType { get; set; } = AccountLegalTypes.Individual;

    [MaxLength(160)]
    public string? OrganizationName { get; set; }

    [MaxLength(20)]
    public string? Inn { get; set; }

    [MaxLength(30)]
    public string? OgrnOrOgrnip { get; set; }

    [MaxLength(250)]
    public string? LegalAddress { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "Нужно принять правила сервиса и политику конфиденциальности")]
    public bool AcceptTerms { get; set; }
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Укажи email")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажи пароль")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
