﻿using System.Collections.ObjectModel;
using Quingo.Shared.Entities;

namespace Quingo.Application.State;

public class GameDrawState : IDisposable
{
    public GameDrawState(GameState gameState, ReadOnlyCollection<PlayerState> players,
        Guid? playerSessionId = null)
    {
        GameState = gameState;
        Players = players;
        PlayerSessionId = playerSessionId;

        _random = new Random(gameState.GameSessionId.GetHashCode());
        _qNodes = [..gameState.QNodes];
        QuestionCount = _qNodes.Count;
        AutoDrawTimer = new GameTimer(Preset.AutoDrawTimer);
        CreatedAt = UpdatedAt = DateTime.UtcNow;

        _drawnNodes.CollectionChanged += HandleDrawnNodesChanged;
        GameState.GameStateChanged += HandleGameStateChanged;
    }

    public GameState GameState { get; }

    private readonly Random _random;

    private PackPresetData Preset => GameState.Preset;

    private GameStateEnum State => GameState.State;

    public ReadOnlyCollection<PlayerState> Players { get; set; }

    public Guid? PlayerSessionId { get; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public int QuestionCount { get; private set; }

    private readonly ObservableCollection<Node> _drawnNodes = [];
    public ReadOnlyCollection<Node> DrawnNodes => _drawnNodes.AsReadOnly();

    private readonly List<Node> _qNodes;
    public ReadOnlyCollection<Node> QNodes => _qNodes.AsReadOnly();

    public bool CanDraw => _qNodes.Count > 0 && State is GameStateEnum.Active;

    public bool PlayerCanDraw(string userId) => PlayerState != null && PlayerState.PlayerUserId == userId;

    public PlayerState? PlayerState =>
        PlayerSessionId.HasValue ? Players.FirstOrDefault(x => x.PlayerSessionId == PlayerSessionId) : null;

    public GameTimer AutoDrawTimer { get; }

    public void ResetAutoDrawTimer(int value)
    {
        AutoDrawTimer.Reset(value).Start();
        NotifyTimerUpdated();
    }

    public void Draw()
    {
        if (!CanDraw) return;

        var idx = _random.Next(0, _qNodes.Count);
        var node = _qNodes[idx];
        _qNodes.Remove(node);
        _drawnNodes.Add(node);
        ResetAutoDrawTimer(Preset.AutoDrawTimer);
    }

    private void HandleGameStateChanged(GameStateEnum state)
    {
        switch (state)
        {
            case GameStateEnum.Active:
                AutoDrawTimer.Start();
                break;
            case GameStateEnum.Paused:
            case GameStateEnum.FinalCountdown:
            case GameStateEnum.Finished:
            case GameStateEnum.Canceled:
                AutoDrawTimer.Stop();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }
    

    public event Action? NodeDrawn;

    public event Action? TimerUpdated;

    private void HandleDrawnNodesChanged(object? sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        NodeDrawn?.Invoke();
    }
    
    public void NotifyTimerUpdated()
    {
        TimerUpdated?.Invoke();
    }

    #region dispose

    private bool _disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        
        NodeDrawn = null;
        TimerUpdated = null;

        _drawnNodes.CollectionChanged -= HandleDrawnNodesChanged;
        GameState.GameStateChanged -= HandleGameStateChanged;

        _disposedValue = true;
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}