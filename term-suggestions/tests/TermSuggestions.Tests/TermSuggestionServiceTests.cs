using TermSuggestions.Interfaces;
using TermSuggestions.Models;
using TermSuggestions.Services;
using Xunit;

namespace TermSuggestions.Tests;

public sealed class TermSuggestionServiceTests
{
    private readonly ITermSuggestionService _service;

    public TermSuggestionServiceTests()
    {
        var calculator = new DifferenceCalculator();
        _service = new TermSuggestionService(calculator);
    }

    [Fact]
    public void GetSuggestions_ExerciseExample_ReturnsCorrectResults()
    {
        var searchTerm = "gros";
        var candidateTerms = new[] { "gros", "gras", "graisse", "agressif", "go", "ros", "gro" };
        var maxSuggestions = 2;

        var results = _service.GetSuggestions(searchTerm, candidateTerms, maxSuggestions);

        Assert.Equal(2, results.Count);
        Assert.Equal("gros", results[0].Term);
        Assert.Equal(0, results[0].DifferenceScore);
        Assert.Equal("gras", results[1].Term);
        Assert.Equal(1, results[1].DifferenceScore);
    }

    [Fact]
    public void GetSuggestions_ExerciseExampleAllResults_ShowsCompleteScoring()
    {
        var searchTerm = "gros";
        var candidateTerms = new[] { "gros", "gras", "graisse", "agressif", "go", "ros", "gro" };
        var maxSuggestions = 10;

        // Act
        var results = _service.GetSuggestions(searchTerm, candidateTerms, maxSuggestions);

        // Assert - Only terms with length >= searchTerm.Length should be included
        Assert.Equal(4, results.Count); // gros, gras, graisse, agressif
        
        // Verify order: by score, then by length, then alphabetically
        Assert.Equal("gros", results[0].Term); // 0 differences
        Assert.Equal(0, results[0].DifferenceScore);
        
        Assert.Equal("gras", results[1].Term); // 1 difference, length 4
        Assert.Equal(1, results[1].DifferenceScore);
        
        Assert.Equal("agressif", results[2].Term); // 1 difference, length 8 (best position: "gres" vs "gros")
        Assert.Equal(1, results[2].DifferenceScore);
        
        Assert.Equal("graisse", results[3].Term); // 2 differences, length 7 (best position: "grai" vs "gros")
        Assert.Equal(2, results[3].DifferenceScore);
    }

    [Fact]
    public void GetSuggestions_CaseInsensitive_WorksCorrectly()
    {
        // Arrange
        var searchTerm = "GROS";
        var candidateTerms = new[] { "Gros", "GRAS", "graisse" };
        var maxSuggestions = 3;

        // Act
        var results = _service.GetSuggestions(searchTerm, candidateTerms, maxSuggestions);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal("gros", results[0].Term); // Should be normalized to lowercase
        Assert.Equal("gras", results[1].Term);
        Assert.Equal("graisse", results[2].Term);
    }

    [Fact]
    public void GetSuggestions_EmptyList_ReturnsEmpty()
    {
        // Arrange
        var searchTerm = "test";
        var candidateTerms = Array.Empty<string>();
        var maxSuggestions = 5;

        // Act
        var results = _service.GetSuggestions(searchTerm, candidateTerms, maxSuggestions);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void GetSuggestions_NoValidCandidates_ReturnsEmpty()
    {
        // Arrange - All candidates are shorter than search term
        var searchTerm = "testing";
        var candidateTerms = new[] { "test", "go", "hi", "a" };
        var maxSuggestions = 5;

        // Act
        var results = _service.GetSuggestions(searchTerm, candidateTerms, maxSuggestions);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void GetSuggestions_TieBreakingByLength_WorksCorrectly()
    {
        // Arrange - Create a scenario where multiple terms have same score
        var searchTerm = "abc";
        var candidateTerms = new[] { "abcdefgh", "abcdef", "abcd" }; // All have 0 differences in overlap
        var maxSuggestions = 3;

        // Act
        var results = _service.GetSuggestions(searchTerm, candidateTerms, maxSuggestions);

        // Assert
        Assert.Equal(3, results.Count);
        
        // All should have score 0, ordered by length
        Assert.All(results, r => Assert.Equal(0, r.DifferenceScore));
        Assert.Equal("abcd", results[0].Term); // Length 4
        Assert.Equal("abcdef", results[1].Term); // Length 6
        Assert.Equal("abcdefgh", results[2].Term); // Length 8
    }

    [Fact]
    public void GetSuggestions_TieBreakingAlphabetically_WorksCorrectly()
    {
        // Arrange - Same score and length, should sort alphabetically
        var searchTerm = "abc";
        var candidateTerms = new[] { "zbc1", "abc1", "bbc1" }; // All length 4, different first chars
        var maxSuggestions = 3;

        // Act
        var results = _service.GetSuggestions(searchTerm, candidateTerms, maxSuggestions);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal("abc1", results[0].Term); // 0 differences
        Assert.Equal("bbc1", results[1].Term); // 1 difference, alphabetically before zbc1
        Assert.Equal("zbc1", results[2].Term); // 1 difference
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void GetSuggestions_InvalidSearchTerm_ThrowsArgumentException(string searchTerm)
    {
        // Arrange
        var candidateTerms = new[] { "test" };
        var maxSuggestions = 1;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.GetSuggestions(searchTerm, candidateTerms, maxSuggestions));
    }

    [Fact]
    public void GetSuggestions_NullCandidateTerms_ThrowsArgumentNullException()
    {
        // Arrange
        var searchTerm = "test";
        IEnumerable<string> candidateTerms = null!;
        var maxSuggestions = 1;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.GetSuggestions(searchTerm, candidateTerms, maxSuggestions));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetSuggestions_InvalidMaxSuggestions_ThrowsArgumentException(int maxSuggestions)
    {
        // Arrange
        var searchTerm = "test";
        var candidateTerms = new[] { "test" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.GetSuggestions(searchTerm, candidateTerms, maxSuggestions));
    }

    [Fact]
    public void GetSuggestions_LargeDataset_PerformsEfficiently()
    {
        // Arrange - Test with large dataset
        var searchTerm = "test";
        var candidateTerms = Enumerable.Range(0, 10000)
            .Select(i => $"test{i:D4}")
            .ToList();
        var maxSuggestions = 100;

        // Act
        var startTime = DateTime.UtcNow;
        var results = _service.GetSuggestions(searchTerm, candidateTerms, maxSuggestions);
        var endTime = DateTime.UtcNow;

        // Assert
        Assert.Equal(100, results.Count);
        Assert.True((endTime - startTime).TotalMilliseconds < 1000, "Should handle large datasets efficiently");
        Assert.All(results, r => Assert.StartsWith("test", r.Term));
    }

    [Fact]
    public void GetSuggestions_FiltersNullAndEmptyTerms_WorksCorrectly()
    {
        // Arrange
        var searchTerm = "test";
        var candidateTerms = new[] { "testing", null!, "", "tested", "   " };
        var maxSuggestions = 5;

        // Act
        var results = _service.GetSuggestions(searchTerm, candidateTerms, maxSuggestions);

        // Assert
        Assert.Equal(2, results.Count); // Only "testing" and "tested" should be valid
        Assert.Contains(results, r => r.Term == "testing");
        Assert.Contains(results, r => r.Term == "tested");
    }
}