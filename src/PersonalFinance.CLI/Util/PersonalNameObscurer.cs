using System.Text.RegularExpressions;

namespace PersonalFinance.CLI.Util;

public static class PersonalNameObscurer
{
    public static string Obscure(string? description, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return description ?? string.Empty;
        }

        var firstToken = Regex.Escape(firstName);
        var lastToken = Regex.Escape(lastName);

        var firstVariant = $@"{firstToken}[A-Za-z]*";
        var middleSuffix = @"(?:[\s\p{P}]+[A-Za-z]\.?)?";
        var separator = @"[\s\p{P}]*";

        var patterns = new[]
        {
            $@"\b{firstVariant}{middleSuffix}{separator}{lastToken}\b",
            $@"\b{lastToken}{separator}{firstVariant}{middleSuffix}\b",
            $@"\b{lastToken}\s*,\s*{firstVariant}{middleSuffix}\b"
        };

        var regex = new Regex(string.Join("|", patterns), RegexOptions.IgnoreCase);
        return regex.Replace(description, "[REDACTED]");
    }
}
