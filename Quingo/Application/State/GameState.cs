﻿using Quingo.Shared.Entities;
using System.Collections.ObjectModel;

namespace Quingo.Application.State;

public class GameState : IDisposable
{
    public GameState(Guid gameSessionId, Pack pack, PackPreset preset, string hostUserId, string? hostName)
    {
        GameSessionId = gameSessionId;
        PackId = pack.Id;
        Pack = pack;
        Preset = preset.Data;
        HostUserId = hostUserId;
        HostName = hostName;
        StartedAt = UpdatedAt = DateTime.UtcNow;
        State = GameStateEnum.Init;
        EndgameTimer = preset.Data.EndgameTimer;

        _random = new Random(gameSessionId.GetHashCode());

        _drawnNodes.CollectionChanged += HandleDrawnNodesChanged;
        _players.CollectionChanged += HandlePlayersChanged;
        _winningPlayers.CollectionChanged += HandleWinningPlayersChanged;

        Setup();
    }

    public Guid GameSessionId { get; private set; }

    public int PackId { get; private set; }

    public Pack Pack { get; private set; }

    public PackPresetData Preset { get; private set; }

    public string HostUserId { get; private set; }

    public string? HostName { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    private readonly Random _random;
    public Random Random => _random;

    private readonly ObservableCollection<Node> _drawnNodes = [];
    public ReadOnlyCollection<Node> DrawnNodes => _drawnNodes.AsReadOnly();

    private List<Node> _qNodes = [];
    public ReadOnlyCollection<Node> QNodes => _qNodes.AsReadOnly();

    private readonly ObservableCollection<PlayerState> _players = [];
    public ReadOnlyCollection<PlayerState> Players => _players.AsReadOnly();

    private readonly ObservableCollection<PlayerState> _winningPlayers = [];
    public ReadOnlyCollection<PlayerState> WinningPlayers => _winningPlayers.AsReadOnly();

    public bool CanDraw => _qNodes.Count > 0;

    private List<bool[,]> _bingoPatterns = [];

    public GameStateEnum State { get; private set; }

    public int EndgameTimer { get; private set; }


    private void Setup()
    {
        if (State != GameStateEnum.Init) return;

        var qTagIds = Preset.Columns.SelectMany(x => x.QuestionTags).Distinct().ToList();
        _qNodes = Pack.Nodes.Where(x => qTagIds.Any(t => x.NodeTags.Select(nt => nt.TagId).Contains(t))).ToList();
        _bingoPatterns = PatternGenerator.GenerateDefaultPatterns(Preset.CardSize);
        State = GameStateEnum.Active;
    }

    public void Join(PlayerState player)
    {
        if (State != GameStateEnum.Active) return;

        _players.Add(player);
    }

    public void Draw()
    {
        if (!CanDraw || State != GameStateEnum.Active) return;

        var idx = _random.Next(0, _qNodes.Count - 1);
        var node = _qNodes[idx];
        _qNodes.Remove(node);
        _drawnNodes.Add(node);
        NotifyStateChanged();
    }

    public void Call(PlayerState player)
    {
        if (player.LivesNumber <= 0 || (State is not GameStateEnum.Active or GameStateEnum.FinalCountdown)) return;

        player.Validate();
        var isValid = false;

        foreach (var pattern in _bingoPatterns)
        {
            var res = new bool[Preset.CardSize, Preset.CardSize];
            var patternValid = true;

            for (var col = 0; col < Preset.CardSize; col++)
            {
                for (var row = 0; row < Preset.CardSize; row++)
                {
                    var patCell = pattern[col, row];
                    var plCell = player.Card.Cells[col, row];
                    patternValid = patCell & plCell.IsMarked & plCell.IsValid == patCell;
                    if (!patternValid) break;
                }
                if (!patternValid) break;
            }

            if (patternValid)
            {
                isValid = true;
                break;
            }
        }

        if (!isValid)
        {
            player.RemoveLife();
        }
        else
        {
            _winningPlayers.Add(player);
            StartCountdown();
        }
    }

    public void EndGame()
    {
        if (State != GameStateEnum.Active) return;

        State = GameStateEnum.Finished;
        NotifyStateChanged();
    }

    private void StartCountdown()
    {
        if (State != GameStateEnum.Active) return;

        State = GameStateEnum.FinalCountdown;
        NotifyStateChanged();

        _ = Task.Run(async () =>
        {
            while (EndgameTimer > 0)
            {
                EndgameTimer--;
                NotifyStateChanged();
                await Task.Delay(1000);
            }

            State = GameStateEnum.Finished;
            NotifyStateChanged();
        });
    }


    #region events
    public event Action? StateChanged;

    public event Action? NodeDrawn;

    private void NotifyStateChanged()
    {
        UpdatedAt = DateTime.UtcNow;
        StateChanged?.Invoke();
    }

    private void HandleDrawnNodesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        NotifyStateChanged();
        NodeDrawn?.Invoke();
    }

    private void HandlePlayersChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        NotifyStateChanged();
    }

    private void HandleWinningPlayersChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        NotifyStateChanged();
    }
    #endregion

    #region dispose
    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            StateChanged = null;
            NodeDrawn = null;
            _drawnNodes.CollectionChanged -= HandleDrawnNodesChanged;
            _players.CollectionChanged -= HandlePlayersChanged;
            _winningPlayers.CollectionChanged -= HandleWinningPlayersChanged;

            foreach (var player in _players)
            {
                player.Dispose();
            }

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

public enum GameStateEnum
{
    Init,
    Active,
    FinalCountdown,
    Finished
}
