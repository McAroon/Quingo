using Quingo.Shared.Entities;

namespace Quingo.Application.State;

public class PlayerState : IDisposable
{
    public PlayerState(Guid playerSessionId, GameState gameState, string playerUserId)
    {
        PlayerSessionId = playerSessionId;
        GameState = gameState;
        Card = new PlayerCard(Preset);
        LivesNumber = Preset.LivesNumber;
        PlayerUserId = playerUserId;
        StartedAt = UpdatedAt = DateTime.UtcNow;

        Setup();
    }

    public Guid PlayerSessionId { get; set; }

    public GameState GameState { get; private set; }

    private Pack Pack => GameState.Pack;

    private PackPresetData Preset => GameState.Preset;

    public PlayerCard Card { get; private set; }

    public int LivesNumber { get; private set; }

    public string PlayerUserId { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    private void Setup()
    {
        if (Preset.Columns.Count != Preset.CardSize) 
        {
            throw new GameStateException("Column number and card size don't match");
        }

        var aTagIds = Preset.Columns.SelectMany(x => x.AnswerTags).Distinct().ToList();
        var aNodes = Pack.Nodes.Where(x => aTagIds.Any(t => x.NodeTags.Select(nt => nt.TagId).Contains(t))).ToList();

        for (var col = 0; col < Card.Cells.GetLength(0); col++)
        {
            var colTagIds = Preset.Columns[col].AnswerTags;
            var colNodes = aNodes.Where(x => colTagIds.Any(t => x.NodeTags.Select(nt => nt.TagId).Contains(t))).ToList();

            for (var row = 0; row < Card.Cells.GetLength(1); row++)
            {
                if (colNodes.Count == 0 || (Preset.FreeCenter && IsCenter(Preset.CardSize, col, row)))
                {
                    Card.Cells[col, row] = new PlayerCardCell(col, row);
                }
                else
                {
                    var idx = GameState.Random.Next(0, colNodes.Count - 1);
                    var node = colNodes[idx];
                    colNodes.Remove(node);
                    aNodes.Remove(node);

                    Card.Cells[col, row] = new PlayerCardCell(col, row, node);
                }
            }
        }
    }

    public void Mark(int col, int row) 
    {
        ArgumentOutOfRangeException.ThrowIfNegative(col, nameof(col));
        ArgumentOutOfRangeException.ThrowIfNegative(row, nameof(row));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(col, Preset.CardSize, nameof(col));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, Preset.CardSize, nameof(row));

        var cell = Card.Cells[col, row];
        cell.IsMarked = !cell.IsMarked;
        if (!cell.IsMarked)
        {
            cell.IsValid = true;
        }
        StateChanged();
    }

    public void Validate()
    {
        for (var col = 0; col < Card.Cells.GetLength(0); col++)
        {
            for (var row = 0; row < Card.Cells.GetLength(1); row++)
            {
                var cell = Card.Cells[col, row];
                if (cell.Node == null || !cell.IsMarked)
                {
                    cell.IsValid = true;
                }
                else
                {
                    cell.IsValid = cell.Node.LinkedNodes.Any(n => GameState.DrawnNodes.Contains(n));
                }
            }
        }
        StateChanged();
    }

    public void RemoveLife()
    {
        if (LivesNumber > 0)
        {
            LivesNumber -= 1;
            StateChanged();
        }
    }

    private static bool IsCenter(int size, int i, int j)
    {
        if (size <= 0 || size % 2 == 0)
            return false;

        return i == j && i == (size - 1) / 2;
    }

    public event EventHandler? OnStateChange;

    private void StateChanged()
    {
        UpdatedAt = DateTime.UtcNow;
        OnStateChange?.Invoke(this, EventArgs.Empty);
    }

    #region dispose
    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            OnStateChange = null;
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}

public class PlayerCard(PackPresetData preset)
{
    public PlayerCardCell[,] Cells { get; } = new PlayerCardCell[preset.CardSize, preset.CardSize];
}

public class PlayerCardCell(int col, int row, Node? node = null)
{
    public int Col { get; } = col;

    public int Row { get; } = row;

    public Node? Node { get; } = node;

    public bool IsFree => Node == null;

    public bool IsMarked { get; set; }

    public bool IsValid { get; set; } = true;
}
