using VocabularyWidget.Models;
using VocabularyWidget.Services;
using Xunit;

namespace VocabularyWidget.Tests;

public class ReviewSchedulerTests
{
    [Fact]
    public void Returns_null_when_empty()
    {
        var scheduler = new ReviewScheduler(new Random(1));
        Assert.Null(scheduler.PickNext(Array.Empty<WordItem>(), null));
    }

    [Fact]
    public void Does_not_repeat_the_only_other_card()
    {
        var a = new WordItem { Id = "a", Word = "a", Definition = "1" };
        var b = new WordItem { Id = "b", Word = "b", Definition = "2" };
        var scheduler = new ReviewScheduler(new Random(42));
        for (int i = 0; i < 20; i++)
        {
            var next = scheduler.PickNext(new[] { a, b }, a);
            Assert.Equal("b", next!.Id);
        }
    }

    [Fact]
    public void Prefers_missed_words()
    {
        var easy = new WordItem { Id = "easy", Word = "easy", Definition = "dễ", ReviewCount = 10, CorrectCount = 10 };
        var hard = new WordItem { Id = "hard", Word = "hard", Definition = "khó", ReviewCount = 10, CorrectCount = 0 };
        int hardHits = 0;
        int trials = 400;
        for (int seed = 0; seed < trials; seed++)
        {
            var scheduler = new ReviewScheduler(new Random(seed));
            var pick = scheduler.PickNext(new[] { easy, hard }, last: null);
            if (pick!.Id == "hard")
            {
                hardHits++;
            }
        }

        Assert.True(hardHits > trials * 0.7, $"expected hard word to dominate, got {hardHits}/{trials}");
    }
}
