using Quingo.Shared.Entities;
using System.Collections.ObjectModel;

namespace Quingo.Application.State;

public class GameState : IDisposable
{
    public GameState(Guid gameSessionId, Pack pack, PackPreset preset, string hostUserId)
    {
        GameSessionId = gameSessionId;
        PackId = pack.Id;
        Pack = pack;
        Preset = preset.Data;
        HostUserId = hostUserId;
        StartedAt = UpdatedAt = DateTime.UtcNow;

        _random = new Random(gameSessionId.GetHashCode());

        _drawnNodes.CollectionChanged += HandleDrawnNodesChanged;
        _players.CollectionChanged += HandlePlayersChanged;

        Setup();
    }

    public Guid GameSessionId { get; private set; }

    public int PackId { get; private set; }

    public Pack Pack { get; private set; }

    public PackPresetData Preset { get; private set; }

    public string HostUserId { get; private set; }

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

    public bool CanDraw => _qNodes.Count > 0;

    private List<bool[,]> _bingoPatterns = [];


    private void Setup()
    {
        var qTagIds = Preset.Columns.SelectMany(x => x.QuestionTags).Distinct().ToList();
        _qNodes = Pack.Nodes.Where(x => qTagIds.Any(t => x.NodeTags.Select(nt => nt.TagId).Contains(t))).ToList();
        _bingoPatterns = PatternGenerator.GenerateDefaultPatterns(Preset.CardSize);
    }

    public void Join(PlayerState player)
    {
        _players.Add(player);
    }

    public void Draw()
    {
        if (!CanDraw) return;

        var idx = _random.Next(0, _qNodes.Count - 1);
        var node = _qNodes[idx];
        _qNodes.Remove(node);
        _drawnNodes.Add(node);
        StateChanged();
    }

    public void Call(PlayerState player)
    {
        if (player.LivesNumber <= 0) return;

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
            // todo: end timer; finish game
        }
    }


    #region events
    public event EventHandler? OnStateChange;

    private void StateChanged()
    {
        UpdatedAt = DateTime.UtcNow;
        OnStateChange?.Invoke(this, EventArgs.Empty);
    }

    private void HandleDrawnNodesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        StateChanged();
    }

    private void HandlePlayersChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        StateChanged();
    }
    #endregion

    #region dispose
    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            OnStateChange = null;
            _drawnNodes.CollectionChanged -= HandleDrawnNodesChanged;
            _players.CollectionChanged -= HandlePlayersChanged;

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
