using Quingo.Shared.Entities;

namespace Quingo.Application.State;

public class PlayerState : IDisposable
{
    public PlayerState(Guid playerSessionId, GameState gameState, string playerUserId, string playerName)
    {
        PlayerSessionId = playerSessionId;
        GameState = gameState;
        Card = new PlayerCardData(Preset);
        LivesNumber = Preset.LivesNumber;
        PlayerUserId = playerUserId;
        PlayerName = playerName;
        StartedAt = UpdatedAt = DateTime.UtcNow;

        Setup();
    }

    public Guid PlayerSessionId { get; set; }

    public GameState GameState { get; private set; }

    private Pack Pack => GameState.Pack;

    private PackPresetData Preset => GameState.Preset;

    public PlayerCardData Card { get; private set; }

    private int _livesNumber;
    public int LivesNumber 
    { 
        get => _livesNumber; 
        private set 
        {
            var decreased = value < _livesNumber;
            _livesNumber = value;
            if (decreased)
            {
                LifeLost?.Invoke(this);
            }
        } 
    }

    public string PlayerUserId { get; private set; }

    public string PlayerName { get; private set; }

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
                    Card.Cells[col, row] = new PlayerCardCellData(col, row);
                }
                else
                {
                    var idx = GameState.Random.Next(0, colNodes.Count - 1);
                    var node = colNodes[idx];
                    colNodes.Remove(node);
                    aNodes.Remove(node);

                    Card.Cells[col, row] = new PlayerCardCellData(col, row, node);
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
        ValidateCell(cell);

        NotifyStateChanged();
    }

    public void Validate()
    {
        for (var col = 0; col < Card.Cells.GetLength(0); col++)
        {
            for (var row = 0; row < Card.Cells.GetLength(1); row++)
            {
                var cell = Card.Cells[col, row];
                ValidateCell(cell);
            }
        }
        NotifyStateChanged();
    }

    private void ValidateCell(PlayerCardCellData cell)
    {
        if (cell.Node == null)
        {
            cell.IsValid = true;
        }
        else
        {
            var found = cell.Node.NodeLinks.Any(n => GameState.DrawnNodes
            .FirstOrDefault(dn => dn.Id != cell.Node.Id && (dn.Id == n.NodeFromId || dn.Id == n.NodeToId)) != null);

            if (!found)
            {
                var indirectLinks = cell.Node.Pack.IndirectLinks
                    .Where(l => l.TagFromId != null && l.TagToId != null)
                    .Where(l => cell.Node.NodeTags.FirstOrDefault(nt => nt.TagId == l.TagFromId) != null);
                foreach (var link in indirectLinks)
                {
                    var toNodes = cell.Node.NodeLinksTo
                        .Where(nl => nl.NodeTo.NodeTags.FirstOrDefault(nt => nt.TagId == link.TagToId) != null)
                        .Select(nl => nl.NodeFrom);
                    var sameTypeNodes = toNodes
                        .SelectMany(n => n.NodeLinksFrom)
                        .Select(nl => nl.NodeTo)
                        .Where(n => n.Id != cell.Node.Id && n.NodeTags.FirstOrDefault(nt => nt.TagId == link.TagFromId) != null);
                    found = GameState.DrawnNodes.Join(sameTypeNodes, x => x.Id, y => y.Id, (x, y) => x).Any();
                    if (found) break;
                }
            }
            cell.IsValid = cell.IsMarked ? found : !found;
        }
    }

    public void RemoveLife()
    {
        if (LivesNumber > 0)
        {
            LivesNumber -= 1;
            NotifyStateChanged();
        }
    }

    private static bool IsCenter(int size, int i, int j)
    {
        if (size <= 0 || size % 2 == 0)
            return false;

        return i == j && i == (size - 1) / 2;
    }

    public event Action? StateChanged;

    public event Action<PlayerState>? LifeLost;

    private void NotifyStateChanged()
    {
        UpdatedAt = DateTime.UtcNow;
        StateChanged?.Invoke();
    }

    #region dispose
    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            StateChanged = null;
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

public class PlayerCardData(PackPresetData preset)
{
    public PlayerCardCellData[,] Cells { get; } = new PlayerCardCellData[preset.CardSize, preset.CardSize];
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
