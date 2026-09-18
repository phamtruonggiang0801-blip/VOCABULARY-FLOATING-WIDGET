using System.Text.RegularExpressions;

namespace VocabularyWidget.Services;

/// <summary>
/// Turns a lexicon Word (which may include variants and POS notes) into text suitable for TTS.
/// </summary>
public static class PronunciationText
{
    public static string ForSpeech(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return string.Empty;
        }

        string primary = word.Split(new[] { '/', '|' }, 2, StringSplitOptions.TrimEntries)[0];
        int paren = primary.IndexOfAny(new[] { '（', '(' });
        if (paren > 0)
        {
            primary = primary[..paren];
        }

        primary = Regex.Replace(primary, @"\s+", "").Trim();
        return primary;
    }
}
