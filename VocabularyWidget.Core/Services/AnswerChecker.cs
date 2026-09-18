using System.Text.RegularExpressions;

namespace VocabularyWidget.Services;

public static class AnswerChecker
{
    private static readonly Regex ExtraSpace = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex TokenSplit = new(@"[\s,;:|/·•\-—–]+", RegexOptions.Compiled);

    public static bool IsCorrect(string input, string expected)
    {
        return string.Equals(Normalize(input), Normalize(expected), StringComparison.Ordinal);
    }

    /// <summary>
    /// Write mode: any single token the user types that equals a token in the expected
    /// string is enough (Vietnamese words split on spaces/punctuation; Hanzi variants on / |).
    /// </summary>
    public static bool MatchesAnyToken(string input, string expected)
    {
        if (IsCorrect(input, expected))
        {
            return true;
        }

        var expectedTokens = Tokenize(expected);
        if (expectedTokens.Count == 0)
        {
            return false;
        }

        foreach (string token in Tokenize(input))
        {
            if (!IsMeaningfulToken(token))
            {
                continue;
            }

            if (expectedTokens.Contains(token, StringComparer.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return ExtraSpace.Replace(value.Trim(), " ").ToLowerInvariant();
    }

    public static IReadOnlyList<string> Tokenize(string value)
    {
        string normalized = Normalize(value);
        if (normalized.Length == 0)
        {
            return Array.Empty<string>();
        }

        return TokenSplit.Split(normalized)
            .Select(t => t.Trim('.', '!', '?', '“', '”', '"', '\'', '（', '）', '(', ')'))
            .Where(t => t.Length > 0)
            .ToList();
    }

    private static bool IsMeaningfulToken(string token)
    {
        if (token.Length == 0)
        {
            return false;
        }

        if (token.Any(c => c >= 0x4E00 && c <= 0x9FFF))
        {
            return true;
        }

        return token.Length >= 2;
    }
}
