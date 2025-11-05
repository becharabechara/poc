using TermSuggestions.Interfaces;
using TermSuggestions.Models;

namespace TermSuggestions.Services;

public sealed class TermSuggestionService : ITermSuggestionService
{
    private readonly IDifferenceCalculator _differenceCalculator;

    public TermSuggestionService(IDifferenceCalculator differenceCalculator)
    {
        _differenceCalculator = differenceCalculator ?? throw new ArgumentNullException(nameof(differenceCalculator));
    }

    public IReadOnlyList<SuggestionResult> GetSuggestions(string searchTerm, IEnumerable<string> candidateTerms, int maxSuggestions)
    {
        if (string.IsNullOrEmpty(searchTerm))
            throw new ArgumentException("Search term cannot be null or empty", nameof(searchTerm));
        
        if (candidateTerms == null)
            throw new ArgumentNullException(nameof(candidateTerms));
            
        if (maxSuggestions <= 0)
            throw new ArgumentException("Max suggestions must be positive", nameof(maxSuggestions));

        var searchTermLower = searchTerm.ToLowerInvariant();
        var results = new List<SuggestionResult>();
        
        foreach (var term in candidateTerms)
        {
            if (string.IsNullOrEmpty(term) || term.Length < searchTermLower.Length)
                continue;
                
            var termLower = term.ToLowerInvariant();
            var score = _differenceCalculator.GetDifferenceScoreWithLengthTolerance(termLower, searchTermLower);
            
            if (score >= 0)
                results.Add(new SuggestionResult(termLower, score));
        }

        return results
            .OrderBy(r => r.DifferenceScore)
            .ThenBy(r => r.Length)
            .ThenBy(r => r.Term, StringComparer.Ordinal)
            .Take(maxSuggestions)
            .ToArray();
    }
}