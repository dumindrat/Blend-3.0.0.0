using System.Globalization;

namespace Blend.BlazorApp.Authentication;

public static class CompanyDateFormats
{
    public const string MonthFirst = "MM/dd/yyyy";
    public const string DayFirst = "dd/MM/yyyy";

    public static bool IsSupported(string? format) => format is MonthFirst or DayFirst;

    public static string Format(string? value, string? format)
    {
        if (string.IsNullOrWhiteSpace(value) || !IsSupported(format)) return value ?? string.Empty;
        var trimmed = value.Trim();
        var suffixIndex = trimmed.IndexOfAny([' ', 'T']);
        var dateText = suffixIndex < 0 ? trimmed : trimmed[..suffixIndex];
        if (!DateOnly.TryParseExact(dateText, [MonthFirst, "M/d/yyyy", "yyyy-MM-dd"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return value;
        var suffix = suffixIndex < 0 ? string.Empty : trimmed[suffixIndex..];
        return date.ToString(format, CultureInfo.InvariantCulture) + suffix;
    }
}
