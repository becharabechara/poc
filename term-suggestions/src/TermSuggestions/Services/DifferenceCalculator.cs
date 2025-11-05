using TermSuggestions.Interfaces;

namespace TermSuggestions.Services;

public sealed class DifferenceCalculator : IDifferenceCalculator
{
    public int GetDifferenceScore(string destination, string source)
    {
        if (string.IsNullOrEmpty(destination) || string.IsNullOrEmpty(source) || 
            destination.Length != source.Length)
            return -1;

        var differences = 0;
        var destSpan = destination.AsSpan();
        var srcSpan = source.AsSpan();
        
        for (var i = 0; i < destSpan.Length; i++)
        {
            if (destSpan[i] != srcSpan[i])
                differences++;
        }

        return differences;
    }

    public int GetDifferenceScoreWithLengthTolerance(string destination, string source)
    {
        if (string.IsNullOrEmpty(destination) || string.IsNullOrEmpty(source) || 
            destination.Length < source.Length)
            return -1;

        var srcSpan = source.AsSpan();
        var destSpan = destination.AsSpan();
        var minDifferences = int.MaxValue;
        var maxStartPos = destSpan.Length - srcSpan.Length;
        
        for (var pos = 0; pos <= maxStartPos; pos++)
        {
            var differences = 0;
            var destSlice = destSpan.Slice(pos, srcSpan.Length);
            
            for (var i = 0; i < srcSpan.Length; i++)
            {
                if (srcSpan[i] != destSlice[i])
                {
                    if (++differences >= minDifferences)
                        break;
                }
            }
            
            if (differences < minDifferences)
            {
                minDifferences = differences;
                if (differences == 0)
                    return 0;
            }
        }

        return minDifferences;
    }
}