using System.Collections.ObjectModel;
using Quingo.Shared.Entities;

namespace Quingo.Application.State;

public class GameDrawState : IDisposable
{
    public GameDrawState(GameState gameState, ReadOnlyCollection<PlayerState> players)
    {
        GameState = gameState;
        Players = players;

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

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public int QuestionCount { get; private set; }

    private readonly ObservableCollection<Node> _drawnNodes = [];
    public ReadOnlyCollection<Node> DrawnNodes => _drawnNodes.AsReadOnly();

    private readonly List<Node> _qNodes;
    public ReadOnlyCollection<Node> QNodes => _qNodes.AsReadOnly();

    public bool CanDraw => _qNodes.Count > 0 && State is GameStateEnum.Active;

    public bool PlayerCanDraw(string userId) => PlayerState != null && PlayerState.PlayerUserId == userId;

    private PlayerState? _playerState;

    public PlayerState? PlayerState
    {
        get => _playerState;
        set
        {
            if (_playerState != null)
                _playerState.StatusChanged -= HandlePlayerStatusChanged;
            _playerState = value;
            if (_playerState != null)
                _playerState.StatusChanged += HandlePlayerStatusChanged;
        }
    }

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
            case GameStateEnum.Active when PlayerState is not { Status: PlayerStatus.Done }:
                AutoDrawTimer.Start();
                if (DrawnNodes.Count == 0)
                {
                    Draw();
                }
                break;
            default:
                AutoDrawTimer.Stop();
                break;
        }
    }

    private void HandlePlayerStatusChanged(PlayerStatus status)
    {
        if (GameState.State is not GameStateEnum.Active) return;
        
        switch (status)
        {
            case PlayerStatus.Ready:
                AutoDrawTimer.Start();
                break;
            default:
                AutoDrawTimer.Stop();
                break;
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
        
        if (PlayerState != null)
        {
            PlayerState.StatusChanged -= HandlePlayerStatusChanged;
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