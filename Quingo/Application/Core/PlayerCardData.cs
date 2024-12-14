using Quingo.Shared.Entities;

namespace Quingo.Application.Core;

public class PlayerCardData(PackPresetData preset)
{
    public PlayerCardCellData[,] Cells { get; } = new PlayerCardCellData[preset.CardSize, preset.CardSize];

    public IEnumerable<PlayerCardCellData> AllCells => Cells.Cast<PlayerCardCellData>();
}

public class PlayerCardCellData(int col, int row, Node? node = null)
{
    public int Col { get; } = col;

    public int Row { get; } = row;

    public Node? Node { get; } = node;

    public bool IsFree => Node == null;

    public bool IsMatchingPattern { get; set; }

    public bool IsMarked { get; set; }

    public bool IsValid { get; set; } = true;

    public bool ShowValidation { get; set; }

    public Node? MatchedQNode { get; set; }

    public PlayerCardCellState State
    {
        get
        {
            if (IsMatchingPattern)
            {
                return PlayerCardCellState.MatchingPattern;
            }
            else if (IsMarked)
            {
                return IsValid ? PlayerCardCellState.Marked : PlayerCardCellState.Invalid;
            }
            else if (!IsValid)
            {
                return PlayerCardCellState.Missing;
            }

            return PlayerCardCellState.Default;
        }
    }
}

public enum PlayerCardCellState
{
    Default,
    Marked,
    Invalid,
    Missing,
    MatchingPattern
}