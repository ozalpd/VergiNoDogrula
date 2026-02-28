using System.Text.RegularExpressions;

namespace VergiNoDogrula.WPF.Helpers;

internal static class TextExtensions
{
    public static bool IsNumeric(this string s)
    {
        return Regex.IsMatch(s, @"^\d+$");
    }

    public static bool IsValidEmail(this string s)
    {
        if (string.IsNullOrEmpty(s))
            return false;

        return emailRegex.IsMatch(s);
    }
    private static Regex emailRegex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,6}$");
}

