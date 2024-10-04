﻿using Quingo.Shared.Entities;

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
        var exclTagIds = Preset.Columns.Where(x => x.ExcludeTags != null).SelectMany(x => x.ExcludeTags).Distinct().ToList();
        var aNodes = Pack.Nodes.Where(x => aTagIds.Any(t => x.HasTag(t)) && exclTagIds.All(t => !x.HasTag(t))).ToList();

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
                    var tagIds = colNodes.SelectMany(x => x.Tags).Select(x => x.Id).Distinct().ToList();
                    var tagIdx = GameState.Random.Next(0, tagIds.Count - 1);
                    var tagId = tagIds[tagIdx];
                    var tagNodes = colNodes.Where(x => x.HasTag(tagId)).ToList();

                    var idx = GameState.Random.Next(0, tagNodes.Count - 1);
                    var node = tagNodes[idx];
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
        if (GameState.Preset.MatchRule is PackPresetMatchRule.LastDrawn && GameState.DrawnNodes.Count > 0)
        {
            cell.MatchedQNode = cell.IsMarked ? GameState.DrawnNodes.Last() : null;
        }

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
            var search = new NodeLinkSearch(cell.Node, GameState.DrawnNodes).Search();
            var found = GameState.Preset.MatchRule is PackPresetMatchRule.Default || !cell.IsMarked 
                ? search.FirstOrDefault() != null 
                : cell.MatchedQNode != null && search.FirstOrDefault(x => x.Id == cell.MatchedQNode.Id) != null;

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
