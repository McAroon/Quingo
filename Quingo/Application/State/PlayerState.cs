using Quingo.Shared.Entities;

namespace Quingo.Application.State;

public class PlayerState : IDisposable
{
    public PlayerState(Guid playerSessionId, GameState gameState, string playerUserId, string playerName,
        GameDrawState drawState)
    {
        PlayerSessionId = playerSessionId;
        GameState = gameState;
        Card = new PlayerCardData(Preset);
        LivesNumber = Preset.LivesNumber;
        PlayerUserId = playerUserId;
        PlayerName = playerName;
        DrawState = drawState;
        StartedAt = UpdatedAt = DateTime.UtcNow;
        Status = IsHost ? PlayerStatus.Ready : PlayerStatus.NotReady;
        Score = new PlayerScore(this);

        Setup();
    }

    public Guid PlayerSessionId { get; }

    public GameState GameState { get; }

    private Pack Pack => GameState.Pack;

    private PackPresetData Preset => GameState.Preset;

    public PlayerCardData Card { get; }

    public GameDrawState DrawState { get; }

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

    public PlayerScore Score { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    private PlayerStatus _status;

    public PlayerStatus Status
    {
        get => _status;
        private set
        {
            if (_status == value) return;
            
            _status = value;
            UpdatedAt = DateTime.UtcNow;
            StatusChanged?.Invoke(value);
        }
    }
    
    public bool IsHost => PlayerUserId == GameState.HostUserId;
    
    public int? DoneTimer { get; private set; }

    private void Setup()
    {
        if (Preset.Columns.Count != Preset.CardSize)
        {
            throw new GameStateException("Column number and card size don't match");
        }

        var random = Preset.SamePlayerCards ? new Random(GameState.GameSessionId.GetHashCode()) : GameState.Random;

        var allPresetTags = Preset.Columns.SelectMany(x => x.ColAnswerTags).Distinct().ToList();
        var exclTagIds = Preset.Columns.Where(x => x.ExcludeTags != null).SelectMany(x => x.ExcludeTags).Distinct()
            .ToList();
        var nodes = Pack.Nodes
            .Where(x => allPresetTags.Any(t => x.HasTag(t.TagId)) && exclTagIds.All(t => !x.HasTag(t))).ToList();
        var tagsCounter = allPresetTags.ToDictionary(x => x.TagId, _ => 0);

        for (var col = 0; col < Card.Cells.GetLength(0); col++)
        {
            var colPresetTags = Preset.Columns[col].ColAnswerTags;
            var colTagIds = colPresetTags.Select(x => x.TagId).ToList();
            var colNodes = nodes.Where(x => colTagIds.Any(t => x.NodeTags.Select(nt => nt.TagId).Contains(t)))
                .ToList();
            var colTagsCounter = Preset.SingleColumnConfig
                ? tagsCounter
                : colPresetTags.ToDictionary(x => x.TagId, _ => 0);

            for (var row = 0; row < Card.Cells.GetLength(1); row++)
            {
                if (colNodes.Count == 0 || (Preset.FreeCenter && IsCenter(Preset.CardSize, col, row)))
                {
                    Card.Cells[col, row] = new PlayerCardCellData(col, row);
                }
                else
                {
                    var nodeTagIds = colNodes.SelectMany(x => x.Tags).Select(x => x.Id)
                        .Where(x => colTagIds.Contains(x))
                        .Distinct().ToList();
                    var nodePresetTags = colPresetTags.Where(x => nodeTagIds.Contains(x.TagId)).ToList();
                    var belowMinTags = nodePresetTags
                        .Where(x => x.ItemsMin != null && colTagsCounter[x.TagId] < x.ItemsMin).ToList();
                    var aboveMaxTags = nodePresetTags
                        .Where(x => x.ItemsMax != null && colTagsCounter[x.TagId] >= x.ItemsMax).ToList();

                    int tagId;
                    if (belowMinTags.Count > 0)
                    {
                        var tagIdx = random.Next(0, belowMinTags.Count);
                        tagId = belowMinTags[tagIdx].TagId;
                    }
                    else
                    {
                        var nodePresetTagsExclMax = nodePresetTags.Except(aboveMaxTags).ToList();
                        var presetTags = nodePresetTagsExclMax.Count > 0 ? nodePresetTagsExclMax : nodePresetTags;
                        var tagIdx = random.Next(0, presetTags.Count);
                        tagId = presetTags[tagIdx].TagId;
                    }

                    var tagNodes = colNodes.Where(x => x.HasTag(tagId)).ToList();
                    colTagsCounter[tagId]++;

                    var idx = random.Next(0, tagNodes.Count);
                    var node = tagNodes[idx];
                    colNodes.Remove(node);
                    nodes.Remove(node);

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
        Mark(cell);
        NotifyStateChanged();
    }

    private void Mark(PlayerCardCellData cell, bool? mark = null)
    {
        cell.IsMarked = mark ?? !cell.IsMarked;
        if (GameState.Preset.MatchRule is PackPresetMatchRule.LastDrawn && DrawState.DrawnNodes.Count > 0)
        {
            if (cell.IsMarked)
            {
                cell.MatchedQNode = DrawState.DrawnNodes.Last();

                var unmarkCells = Card.AllCells
                    .Where(x => x != cell && x.IsMarked && x.MatchedQNode == cell.MatchedQNode);
                foreach (var unmarkCell in unmarkCells)
                {
                    Mark(unmarkCell, false);
                }
            }
            else
            {
                cell.MatchedQNode = null;
            }
        }

        if (cell is { IsMarked: false, ShowValidation: true })
        {
            cell.ShowValidation = false;
        }

        var drawnNodes = new List<Node>(DrawState.DrawnNodes);
        ValidateCell(cell, drawnNodes);
        Score.Calculate();
    }

    public void Validate(bool isCall = false)
    {
        var drawnNodes = new List<Node>(DrawState.DrawnNodes);
        for (var col = 0; col < Card.Cells.GetLength(0); col++)
        {
            for (var row = 0; row < Card.Cells.GetLength(1); row++)
            {
                var cell = Card.Cells[col, row];
                ValidateCell(cell, drawnNodes, isCall);
            }
        }

        Score.Calculate();
        NotifyStateChanged();
    }

    private void ValidateCell(PlayerCardCellData cell, IList<Node> drawnNodes, bool isCall = false)
    {
        if (cell.Node == null)
        {
            cell.IsValid = true;
        }
        else
        {
            var search = new NodeLinkSearch(cell.Node, DrawState.DrawnNodes).Search();
            var found = GameState.Preset.MatchRule is PackPresetMatchRule.Default || !cell.IsMarked
                ? search.FirstOrDefault() != null
                : cell.MatchedQNode != null && search.FirstOrDefault(x => x.Id == cell.MatchedQNode.Id) != null;

            cell.IsValid = cell.IsMarked ? found : !found;

            if (Preset.Pattern == PackPresetPattern.FullCard)
            {
                cell.IsMatchingPattern = cell is { IsMarked: true, IsValid: true };
            }

            if (isCall && cell is { IsMarked: true, MatchedQNode: not null })
            {
                cell.ShowValidation = true;
            }
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

    public void SetStatus(PlayerStatus status)
    {
        Status = status;
        DoneTimer = status is PlayerStatus.Done ? GameState.Timer.Value : null;
        NotifyStateChanged();
        GameState.NotifyStateChanged();
    }

    private static bool IsCenter(int size, int i, int j)
    {
        if (size <= 0 || size % 2 == 0)
            return false;

        return i == j && i == (size - 1) / 2;
    }

    public event Action? StateChanged;

    public event Action<PlayerState>? LifeLost;

    public event Action<GameState, PlayerState>? NewGameCreated;

    public event Action<PlayerStatus>? StatusChanged;

    private void NotifyStateChanged()
    {
        UpdatedAt = DateTime.UtcNow;
        StateChanged?.Invoke();
    }

    public void NotifyNewGameCreated(GameState game, PlayerState player)
    {
        NewGameCreated?.Invoke(game, player);
    }

    #region dispose

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            StateChanged = null;
            LifeLost = null;
            NewGameCreated = null;
            StatusChanged = null;
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

public enum PlayerStatus
{
    NotReady,
    Ready,
    Done
}