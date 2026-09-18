using VocabularyWidget.Models;

namespace VocabularyWidget.Services;

public sealed class MultipleChoiceQuestion
{
    public required string Prompt { get; init; }
    public required string CorrectAnswer { get; init; }
    public required IReadOnlyList<string> Options { get; init; }

    public bool IsCorrect(string choice) =>
        string.Equals(choice, CorrectAnswer, StringComparison.Ordinal);
}

public static class MultipleChoiceBuilder
{
    public const int DefaultOptionCount = 4;

    public static MultipleChoiceQuestion Build(
        IReadOnlyList<WordItem> words,
        WordItem current,
        bool showingWord,
        Random random,
        int optionCount = DefaultOptionCount)
    {
        string prompt = showingWord ? current.Word : current.Definition;
        string correct = showingWord ? current.Definition : current.Word;

        var distractors = words
            .Where(w => w.Id != current.Id)
            .Select(w => showingWord ? w.Definition : w.Word)
            .Where(text => !string.IsNullOrWhiteSpace(text) && text != correct)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(_ => random.Next())
            .Take(Math.Max(0, optionCount - 1))
            .ToList();

        var options = new List<string>(distractors.Count + 1) { correct };
        options.AddRange(distractors);
        Shuffle(options, random);

        return new MultipleChoiceQuestion
        {
            Prompt = prompt,
            CorrectAnswer = correct,
            Options = options
        };
    }

    private static void Shuffle<T>(IList<T> list, Random random)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
