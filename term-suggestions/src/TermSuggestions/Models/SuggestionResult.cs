namespace TermSuggestions.Models;

public sealed class SuggestionResult
{
    public string Term { get; }
    public int DifferenceScore { get; }
    public int Length => Term.Length;

    public SuggestionResult(string term, int differenceScore)
    {
        Term = term ?? throw new ArgumentNullException(nameof(term));
        DifferenceScore = differenceScore;
    }

    public override string ToString() => $"{Term} (score: {DifferenceScore})";
    
    public override bool Equals(object? obj) => 
        obj is SuggestionResult other && Term == other.Term && DifferenceScore == other.DifferenceScore;
    
    public override int GetHashCode() => HashCode.Combine(Term, DifferenceScore);
}