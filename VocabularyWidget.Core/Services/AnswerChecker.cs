using System.Text.RegularExpressions;

namespace VocabularyWidget.Services;

public static class AnswerChecker
{
    private static readonly Regex ExtraSpace = new(@"\s+", RegexOptions.Compiled);

    public static bool IsCorrect(string input, string expected)
    {
        return string.Equals(Normalize(input), Normalize(expected), StringComparison.Ordinal);
    }

    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return ExtraSpace.Replace(value.Trim(), " ").ToLowerInvariant();
    }
}
