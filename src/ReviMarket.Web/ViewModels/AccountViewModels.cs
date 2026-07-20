using System.ComponentModel.DataAnnotations;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.ViewModels;

public class RegisterViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Укажи имя")]
    [MaxLength(60)]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажи email")]
    [EmailAddress(ErrorMessage = "Укажи корректный email")]
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

    public bool AcceptTerms { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!AcceptTerms)
        {
            yield return new ValidationResult("Нужно принять правила сервиса и политику конфиденциальности.", new[] { nameof(AcceptTerms) });
        }

        foreach (var result in AccountBusinessValidation.ValidateBusinessFields(LegalType, OrganizationName, Inn))
        {
            yield return result;
        }
    }
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Укажи email")]
    [EmailAddress(ErrorMessage = "Укажи корректный email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажи пароль")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class ProfileEditViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Укажи имя")]
    [MaxLength(60)]
    public string DisplayName { get; set; } = string.Empty;

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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in AccountBusinessValidation.ValidateBusinessFields(LegalType, OrganizationName, Inn))
        {
            yield return result;
        }
    }
}

internal static class AccountBusinessValidation
{
    public static IEnumerable<ValidationResult> ValidateBusinessFields(string legalType, string? organizationName, string? inn)
    {
        if (legalType != AccountLegalTypes.Business)
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(organizationName))
        {
            yield return new ValidationResult("Для ИП или юрлица укажи название.", new[] { nameof(RegisterViewModel.OrganizationName) });
        }

        if (string.IsNullOrWhiteSpace(inn))
        {
            yield return new ValidationResult("Для ИП или юрлица укажи ИНН.", new[] { nameof(RegisterViewModel.Inn) });
        }
        else
        {
            var normalizedInn = new string(inn.Where(char.IsDigit).ToArray());
            if (normalizedInn.Length is not (10 or 12))
            {
                yield return new ValidationResult("ИНН должен содержать 10 или 12 цифр.", new[] { nameof(RegisterViewModel.Inn) });
            }
        }
    }
}
