﻿using Quingo.Infrastructure;
using Quingo.Shared.Entities;
using Quingo.Shared.Models;
using System.Collections.ObjectModel;

namespace Quingo.Application.Core;

public class GameInstance : IDisposable
{
    public GameInstance(Guid gameSessionId, Pack pack, PackPresetData preset, string hostUserId,
        string? hostName, UserConnectionTracker userTracker, GameOptions options,
        int? lobbyId = null, bool isTournament = false)
    {
        GameSessionId = gameSessionId;
        PackId = pack.Id;
        Pack = pack;
        Preset = preset;
        HostUserId = hostUserId;
        HostName = hostName;
        UserTracker = userTracker;
        Options = options;
        LobbyId = lobbyId;
        IsTournament = isTournament;

        StartedAt = UpdatedAt = DateTime.UtcNow;
        State = GameStateEnum.Init;

        Timer = new GameTimer(preset.GameTimer);
        _random = new Random(gameSessionId.GetHashCode());

        _players.CollectionChanged += HandlePlayersChanged;
        _winningPlayers.CollectionChanged += HandleWinningPlayersChanged;
        _spectators.CollectionChanged += HandleSpectatorsChanged;

        NotifyStateChanged = ((Action)NotifyStateChangedUndebounced).Debounce(100);

        Setup();
    }

    public Guid GameSessionId { get; private set; }

    public int PackId { get; private set; }

    public Pack Pack { get; }

    public PackPresetData Preset { get; }

    public string HostUserId { get; private set; }

    public string? HostName { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    private readonly Random _random;
    public Random Random => _random;

    public List<Node> QNodes { get; private set; }

    private readonly ObservableCollection<PlayerInstance> _players = [];
    public ReadOnlyCollection<PlayerInstance> Players => _players.AsReadOnly();

    private readonly ObservableCollection<PlayerInstance> _winningPlayers = [];
    public ReadOnlyCollection<PlayerInstance> WinningPlayers => _winningPlayers.AsReadOnly();

    private readonly ObservableCollection<ApplicationUserInfo> _spectators = [];
    public ReadOnlyCollection<ApplicationUserInfo> Spectators => _spectators.AsReadOnly();

    public bool ShowTimer => Preset.GameTimer > 0 || (Preset.EndgameTimer > 0 && State == GameStateEnum.FinalCountdown);

    private CardPattern _bingoPattern = default!;

    private GameStateEnum _state;

    public int? LobbyId { get; private set; }
    public bool IsTournament { get; private set; }

    public GameStateEnum State
    {
        get => _state;
        private set
        {
            if (_state == value) return;

            _state = value;
            UpdatedAt = DateTime.UtcNow;
            GameStateChanged?.Invoke(value);
        }
    }

    public bool IsStateActive => State is GameStateEnum.Init or GameStateEnum.Active or GameStateEnum.Paused
        or GameStateEnum.FinalCountdown;

    public GameTimer Timer { get; }

    public (int, int) TimerColorPercentages => State switch
    {
        GameStateEnum.FinalCountdown => (0, 101),
        GameStateEnum.Finished or GameStateEnum.Canceled => (0, 0),
        _ => (25, 10),
    };

    public string StateDisplayValue => State switch
    {
        GameStateEnum.Init => "Not Started",
        GameStateEnum.Active => "Active",
        GameStateEnum.Paused => "Paused",
        GameStateEnum.FinalCountdown => "Countdown",
        GameStateEnum.Finished => "Finished",
        GameStateEnum.Canceled => "Finished",
        _ => ""
    };

    private readonly List<GameDrawState> _drawStates = [];
    public ReadOnlyCollection<GameDrawState> DrawStates => _drawStates.AsReadOnly();

    public bool HostCanDraw => _drawStates.Count == 1 && _drawStates[0].PlayerState is null;

    public GameDrawState? DrawState => HostCanDraw ? DrawStates.FirstOrDefault() : null;

    public bool AllPlayersReady => Players.All(x => x.Status is PlayerStatus.Ready);

    public bool AllPlayersDone => Players.All(x => x.Status is PlayerStatus.Done);

    public UserConnectionTracker UserTracker { get; }

    public GameOptions Options { get; }

    public void ResetGameTimer(int value)
    {
        Timer.Reset(value).Start();
        NotifyTimerUpdated();
    }

    public void RefreshGameTimers()
    {
        if (Timer.IsRunning)
            NotifyTimerUpdated();

        foreach (var drawState in _drawStates.Where(x => x.AutoDrawTimer.IsRunning))
        {
            drawState.NotifyTimerUpdated();
        }
    }

    public void PauseGame()
    {
        if (State is not GameStateEnum.Active) return;

        Timer.Stop();
        SetState(GameStateEnum.Paused);
    }

    public void ResumeGame()
    {
        if (State is not GameStateEnum.Paused and not GameStateEnum.Init) return;

        Timer.Start();
        SetState(GameStateEnum.Active);
    }

    public bool CanJoin(string? playerId)
    {
        var s = Preset.MaxPlayers <= 0 || Players.Count < Preset.MaxPlayers 
            || (!string.IsNullOrEmpty(playerId)
            && Players.FirstOrDefault(x => x.PlayerUserId == playerId) != null);

        return s;
    }

    private void Setup()
    {
        if (State != GameStateEnum.Init) return;

        var qTagIds = Preset.Columns.SelectMany(x => x.QuestionTags).Distinct().ToList();
        var exclTagIds = Preset.Columns.Where(x => x.ExcludeTags != null).SelectMany(x => x.ExcludeTags).Distinct()
            .ToList();
        QNodes = Pack.Nodes.Where(x => qTagIds.Any(t => x.HasTag(t)) && exclTagIds.All(t => !x.HasTag(t))).ToList();
        
        if (Preset.MinDifficulty > 0)
        {
            QNodes = QNodes.Where(x => x.Difficulty >= Preset.MinDifficulty).ToList();
        }

        if (Preset.MaxDifficulty > 0 && Preset.MaxDifficulty >= (Preset.MinDifficulty ?? 0))
        {
            QNodes = QNodes.Where(x => x.Difficulty <= Preset.MaxDifficulty).ToList();
        }
        
        _bingoPattern = PatternGenerator.GeneratePatterns(Preset.CardSize, Preset.Pattern);

        if (!Preset.SeparateDrawPerPlayer)
        {
            var drawState = new GameDrawState(this, Players);
            _drawStates.Add(drawState);
        }
    }

    public PlayerInstance Join(string playerUserId, string playerName)
    {
        var playerSessionId = Guid.NewGuid();
        var drawState = Preset.SeparateDrawPerPlayer
            ? new GameDrawState(this, Players)
            : _drawStates.First();
        if (!_drawStates.Contains(drawState))
        {
            _drawStates.Add(drawState);
        }

        var player = new PlayerInstance(playerSessionId, this, playerUserId, playerName, drawState);
        if (Preset.SeparateDrawPerPlayer)
        {
            drawState.PlayerState = player;
            if (State is GameStateEnum.Active)
            {
                if (Preset.AutoDrawTimer > 0)
                {
                    drawState.ResetAutoDrawTimer(Preset.AutoDrawTimer * 2);
                }
                else
                {
                    drawState.Draw();
                }
            }
        }

        _players.Add(player);
        return player;
    }

    public bool CanSpectate(string? userId)
    {
        var s = !string.IsNullOrEmpty(userId) && Players.FirstOrDefault(x => x.PlayerUserId == userId) == null;
        return s;
    }

    public void Spectate(ApplicationUserInfo user)
    {
        if (_spectators.FirstOrDefault(x => x.UserId == user.UserId) == null)
        {
            _spectators.Add(user);
        }
    }

    public void Draw()
    {
        foreach (var drawState in _drawStates)
        {
            drawState.Draw();
        }

        ResumeGame();

        NotifyStateChanged();
    }

    public void Call(PlayerInstance player)
    {
        if (player.LivesNumber <= 0
            || !Preset.EnableCall
            || WinningPlayers.Contains(player)
            || !IsStateActive) return;

        var isValid = ValidatePatterns(player, true);
        if (isValid)
        {
            _winningPlayers.Add(player);
        }
        else
        {
            player.RemoveLife();
        }
    }

    private bool ValidatePatterns(PlayerInstance player, bool isCall = false)
    {
        player.Validate(isCall);

        var validPatternIdx = _bingoPattern.Validate(player.Card).FirstOrDefault();
        if (validPatternIdx == null)
        {
            return false;
        }

        var validPattern = _bingoPattern.Patterns[validPatternIdx.Value];
        for (var col = 0; col < Preset.CardSize; col++)
        {
            for (var row = 0; row < Preset.CardSize; row++)
            {
                var patCell = validPattern[col, row];
                var plCell = player.Card.Cells[col, row];
                plCell.IsMatchingPattern = patCell;
            }
        }

        return true;
    }

    public void EndGame()
    {
        if (!IsStateActive) return;

        SetState(GameStateEnum.Canceled);
    }

    public void SetState(GameStateEnum state)
    {
        if (State == state) return;

        State = state;
        OnGameFinished();
        NotifyStateChanged();
    }

    private void OnGameFinished()
    {
        if (IsStateActive) return;

        Timer.Stop();

        foreach (var player in Players)
        {
            player.Validate();
        }

        if (WinningPlayers.Count == 0 && Players.Count > 0)
        {
            var validPatternPlayerIds = Players.Where(x => ValidatePatterns(x)).Select(x => x.PlayerUserId).ToList();
            if (validPatternPlayerIds.Count > 0)
            {
                SetWinningPlayers(validPatternPlayerIds);
            }
            else
            {
                var maxScore = Players.Select(x => x.Score.ScoreTotal).Max();
                var maxScorePlayerIds = Players
                    .Where(x => x.Score.ScoreTotal == maxScore)
                    .Select(x => x.PlayerUserId).ToList();
                SetWinningPlayers(maxScorePlayerIds);
            }
        }
    }

    private void SetWinningPlayers(List<string> playerIds)
    {
        var playersToAdd = _players.Where(x => playerIds.Contains(x.PlayerUserId) && !_winningPlayers.Contains(x))
            .ToList();
        foreach (var player in playersToAdd)
        {
            _winningPlayers.Add(player);
        }
    }

    #region events

    public event Action? StateChanged;

    public event Action<PlayerInstance>? PlayerJoined;

    public event Action<GameStateEnum>? GameStateChanged;

    public event Action<GameInstance>? NewGameCreated;

    public event Action? TimerUpdated;

    public void NotifyNewGameCreated(GameInstance game)
    {
        NewGameCreated?.Invoke(game);
    }

    public Action NotifyStateChanged { get; }

    private void NotifyStateChangedUndebounced()
    {
        UpdatedAt = DateTime.UtcNow;
        StateChanged?.Invoke();
    }

    private void HandlePlayersChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        NotifyStateChanged();
        var player = e.NewItems?.Count > 0 ? (PlayerInstance?)e.NewItems[0] : null;
        if (player != null)
            PlayerJoined?.Invoke(player);
    }

    private void HandleWinningPlayersChanged(object? sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        NotifyStateChanged();
    }

    private void HandleSpectatorsChanged(object? sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // NotifyStateChanged();
    }

    private void NotifyTimerUpdated()
    {
        TimerUpdated?.Invoke();
    }

    #endregion

    #region dispose

    private bool _disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue) return;

        StateChanged = null;
        PlayerJoined = null;
        GameStateChanged = null;
        NewGameCreated = null;
        TimerUpdated = null;
        _players.CollectionChanged -= HandlePlayersChanged;
        _winningPlayers.CollectionChanged -= HandleWinningPlayersChanged;
        _spectators.CollectionChanged -= HandleSpectatorsChanged;

        foreach (var player in _players)
        {
            player.Dispose();
        }

        foreach (var drawState in _drawStates)
        {
            drawState.Dispose();
        }

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

public enum GameStateEnum
{
    Init,
    Active,
    Paused,
    FinalCountdown,
    Finished,
    Canceled
}