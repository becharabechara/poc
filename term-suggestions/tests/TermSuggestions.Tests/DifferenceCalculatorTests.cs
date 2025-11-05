using TermSuggestions.Services;
using Xunit;

namespace TermSuggestions.Tests;

public sealed class DifferenceCalculatorTests
{
    private readonly DifferenceCalculator _calculator = new();

    [Theory]
    [InlineData("gros", "gros", 0)]
    [InlineData("gras", "gros", 1)]
    [InlineData("test", "best", 1)]
    [InlineData("test", "tast", 1)]
    [InlineData("test", "tesk", 1)]
    [InlineData("hello", "world", 4)]
    [InlineData("abcd", "dcba", 4)]
    public void GetDifferenceScore_SameLengthStrings_ReturnsCorrectScore(string dest, string src, int expected)
    {
        var result = _calculator.GetDifferenceScore(dest, src);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello", "hi", -1)]
    [InlineData("short", "verylongstring", -1)]
    [InlineData("", "test", -1)]
    [InlineData("test", "", -1)]
    [InlineData("", "", -1)]
    public void GetDifferenceScore_InvalidInputs_ReturnsMinusOne(string dest, string src, int expected)
    {
        var result = _calculator.GetDifferenceScore(dest, src);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("gros", "gros", 0)]
    [InlineData("gras", "gros", 1)]
    [InlineData("graisse", "gros", 2)]
    [InlineData("agressif", "gros", 1)]
    public void GetDifferenceScoreWithLengthTolerance_ExerciseExamples_ReturnsCorrectScore(string dest, string src, int expected)
    {
        var result = _calculator.GetDifferenceScoreWithLengthTolerance(dest, src);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("go", "gros", -1)]
    [InlineData("ro", "gros", -1)]
    [InlineData("", "gros", -1)]
    public void GetDifferenceScoreWithLengthTolerance_DestinationShorter_ReturnsMinusOne(string dest, string src, int expected)
    {
        var result = _calculator.GetDifferenceScoreWithLengthTolerance(dest, src);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetDifferenceScore_NullInputs_ReturnsMinusOne()
    {
        Assert.Equal(-1, _calculator.GetDifferenceScore(null!, "test"));
        Assert.Equal(-1, _calculator.GetDifferenceScore("test", null!));
        Assert.Equal(-1, _calculator.GetDifferenceScore(null!, null!));
    }

    [Fact]
    public void GetDifferenceScoreWithLengthTolerance_NullInputs_ReturnsMinusOne()
    {
        Assert.Equal(-1, _calculator.GetDifferenceScoreWithLengthTolerance(null!, "test"));
        Assert.Equal(-1, _calculator.GetDifferenceScoreWithLengthTolerance("test", null!));
        Assert.Equal(-1, _calculator.GetDifferenceScoreWithLengthTolerance(null!, null!));
    }

    [Fact]
    public void GetDifferenceScore_LargeStrings_PerformsEfficiently()
    {
        var largeString1 = new string('a', 10000);
        var largeString2 = new string('b', 10000);

        var startTime = DateTime.UtcNow;
        var result = _calculator.GetDifferenceScore(largeString1, largeString2);
        var endTime = DateTime.UtcNow;

        Assert.Equal(10000, result);
        Assert.True((endTime - startTime).TotalMilliseconds < 100);
    }
}