namespace TermSuggestions.Interfaces;

public interface IDifferenceCalculator
{
    int GetDifferenceScore(string destination, string source);
    int GetDifferenceScoreWithLengthTolerance(string destination, string source);
}