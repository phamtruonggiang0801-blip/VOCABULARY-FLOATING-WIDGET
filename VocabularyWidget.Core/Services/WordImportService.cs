using VocabularyWidget.Models;

namespace VocabularyWidget.Services;

public static class WordImportService
{
    public static List<WordItem> Parse(string text, string? fileName = null)
    {
        var items = new List<WordItem>();
        if (string.IsNullOrWhiteSpace(text))
        {
            return items;
        }

        bool looksCsv = fileName?.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                        ?? (text.Contains(',') && !text.Contains('|') && !text.Contains('\t'));

        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        int start = 0;
        if (lines.Length > 0 && IsHeader(lines[0]))
        {
            start = 1;
        }

        for (int i = start; i < lines.Length; i++)
        {
            if (TryParseLine(lines[i], looksCsv, out var word, out var definition))
            {
                items.Add(new WordItem { Word = word, Definition = definition });
            }
        }

        return items;
    }

    private static bool IsHeader(string line)
    {
        var lower = line.Trim().ToLowerInvariant();
        return lower is "word,definition" or "word,meaning" or "word|definition"
            or "word\tdefinition" or "từ,nghĩa" or "tu,nghia";
    }

    private static bool TryParseLine(string line, bool preferCsv, out string word, out string definition)
    {
        word = string.Empty;
        definition = string.Empty;

        string[] parts;
        if (preferCsv && line.Contains(','))
        {
            parts = SplitCsv(line);
        }
        else if (line.Contains('\t'))
        {
            parts = line.Split('\t', 2);
        }
        else if (line.Contains('|'))
        {
            parts = line.Split('|', 2);
        }
        else if (line.Contains(" - "))
        {
            parts = line.Split(new[] { " - " }, 2, StringSplitOptions.None);
        }
        else if (line.Contains(','))
        {
            parts = SplitCsv(line);
        }
        else
        {
            return false;
        }

        if (parts.Length < 2)
        {
            return false;
        }

        word = Unquote(parts[0]).Trim();
        definition = Unquote(parts[1]).Trim();
        return word.Length > 0 && definition.Length > 0;
    }

    private static string[] SplitCsv(string line)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result.ToArray();
    }

    private static string Unquote(string value)
    {
        value = value.Trim();
        if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
        {
            return value[1..^1].Replace("\"\"", "\"");
        }

        return value;
    }
}
