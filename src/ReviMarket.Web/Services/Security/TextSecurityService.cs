using System.Text.RegularExpressions;

namespace ReviMarket.Web.Services.Security;

public sealed class TextSecurityService
{
    private static readonly Regex DangerousPattern = new(
        @"(<\s*script|javascript\s*:|data\s*:\s*text/html|onerror\s*=|onload\s*=|<\s*iframe|<\s*object|<\s*embed)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(250));

    private static readonly Regex ControlChars = new(@"[\u0000-\u0008\u000B\u000C\u000E-\u001F\u007F]", RegexOptions.Compiled);

    public string CleanText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().Normalize();
        normalized = ControlChars.Replace(normalized, string.Empty);
        normalized = Regex.Replace(normalized, @"[ \t]{2,}", " ");

        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength];
    }

    public bool LooksDangerous(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && DangerousPattern.IsMatch(value);
    }
}
