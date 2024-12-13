using Quingo.Shared.Entities;

namespace Quingo.Application.State;

public class CardPattern(int size, PackPresetPattern patternType, IReadOnlyList<bool[,]> patterns)
{
    public int Size => size;

    public PackPresetPattern PatternType => patternType;

    public IReadOnlyList<bool[,]> Patterns => patterns;

    public IEnumerable<int?> Validate(PlayerCardData card)
    {
        foreach (var (pattern, idx) in patterns.Select((x, i) => (x, i)))
        {
            var patternValid = true;

            for (var col = 0; col < pattern.GetLength(0); col++)
            {
                for (var row = 0; row < pattern.GetLength(1); row++)
                {
                    var patCell = pattern[col, row];
                    var plCell = card.Cells[col, row];
                    patternValid = (patCell & plCell.IsMarked & plCell.IsValid) == patCell;
                    if (!patternValid) break;
                }

                if (!patternValid) break;
            }

            if (patternValid)
            {
                yield return idx;
            }
        }
    }
}