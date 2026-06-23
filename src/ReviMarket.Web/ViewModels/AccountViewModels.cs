using System.ComponentModel.DataAnnotations;

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
    public string AccountType { get; set; } = "Customer";
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
