using TermSuggestions.Models;

namespace TermSuggestions.Interfaces;

public interface ITermSuggestionService
{
    IReadOnlyList<SuggestionResult> GetSuggestions(string searchTerm, IEnumerable<string> candidateTerms, int maxSuggestions);
}