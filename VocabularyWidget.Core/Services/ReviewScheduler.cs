using VocabularyWidget.Models;

namespace VocabularyWidget.Services;

public sealed class ReviewScheduler
{
    private readonly Random _random;

    public ReviewScheduler(Random? random = null)
    {
        _random = random ?? Random.Shared;
    }

    /// <summary>
    /// Weighted pick: more misses → higher chance. Avoids repeating the last card when possible.
    /// </summary>
    public WordItem? PickNext(IReadOnlyList<WordItem> words, WordItem? last)
    {
        if (words.Count == 0)
        {
            return null;
        }

        IReadOnlyList<WordItem> pool = words;
        if (last != null && words.Count > 1)
        {
            var filtered = words.Where(w => !ReferenceEquals(w, last) && w.Id != last.Id).ToList();
            if (filtered.Count > 0)
            {
                pool = filtered;
            }
        }

        int totalWeight = 0;
        var weights = new int[pool.Count];
        for (int i = 0; i < pool.Count; i++)
        {
            weights[i] = 1 + (3 * pool[i].MissCount);
            totalWeight += weights[i];
        }

        int roll = _random.Next(totalWeight);
        int cumulative = 0;
        for (int i = 0; i < pool.Count; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative)
            {
                return pool[i];
            }
        }

        return pool[^1];
    }

    public static bool CoinFlipShowWord(Random? random = null)
    {
        return (random ?? Random.Shared).Next(2) == 0;
    }
}
