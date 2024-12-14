using System.Collections.Concurrent;
using Quingo.Shared.Entities;

namespace Quingo.Application.Core;

public static class PatternGenerator
{
    private static readonly ConcurrentDictionary<(int size, PackPresetPattern pattern), CardPattern> Cache = [];
    
    public static CardPattern GeneratePatterns(int size, PackPresetPattern patternType)
    {
        var key = (size, pattern: patternType);
        if (Cache.TryGetValue(key, out var cached))
        {
            return cached;
        }
        
        var generated = patternType switch
        {
            PackPresetPattern.Lines => GenerateLinePatterns(size),
            PackPresetPattern.FullCard => GenerateFullCardPattern(size),
            _ => throw new InvalidOperationException($"Pattern {patternType} is not supported")
        };

        var pattern = new CardPattern(size, patternType, generated);
        Cache[key] = pattern;

        return pattern;
    }
    private static List<bool[,]> GenerateFullCardPattern(int size)
    {
        var pattern = new bool[size, size];
        for (int col = 0; col < pattern.GetLength(0); col++)
        {
            for (int row = 0; row < pattern.GetLength(1); row++)
            {
                pattern[col, row] = true;
            }
        }

        return [pattern];
    }

    private static List<bool[,]> GenerateLinePatterns(int size)
    {
        var result = new List<bool[,]>();

        // verticals
        for (var i = 0; i < size; i++)
        {
            var pattern = new bool[size, size];
            result.Add(pattern);

            for (int col = 0; col < pattern.GetLength(0); col++)
            {
                for (int row = 0; row < pattern.GetLength(1); row++)
                {
                    pattern[col, row] = col == i;
                }
            }
        }

        // horizontals
        for (var i = 0; i < size; i++)
        {
            var pattern = new bool[size, size];
            result.Add(pattern);

            for (int col = 0; col < pattern.GetLength(0); col++)
            {
                for (int row = 0; row < pattern.GetLength(1); row++)
                {
                    pattern[col, row] = row == i;
                }
            }
        }

        // diagonals
        for (var i = 0; i < 2; i++)
        {
            var pattern = new bool[size, size];
            result.Add(pattern);

            for (int col = 0; col < pattern.GetLength(0); col++)
            {
                for (int row = 0; row < pattern.GetLength(1); row++)
                {
                    if (i == 0)
                    {
                        pattern[col, row] = col == row;
                    }
                    else
                    {
                        pattern[col, row] = col == size - row - 1;
                    }
                }
            }
        }

        return result;
    }
}
