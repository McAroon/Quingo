using System.Collections.Concurrent;

namespace Quingo.Application.State;

public class GameLoop : IDisposable
{
    private const int RemoveFinishedAfterMin = 5;
    private const int RemoveInactiveAfterMin = 60;
    private const int LoopPeriodMs = 1000;
    private const int ParallelEntries = 10;

    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<Guid, GameState> _state;
    private Timer? _timer;
    private Guid _id;

    public GameLoop(ILogger logger, ConcurrentDictionary<Guid, GameState> state)
    {
        _logger = logger;
        _state = state;
        Start();
    }

    public GameLoopState State { get; private set; }

    public DateTime Heartbeat { get; private set; }

    public DateTime LastHeartbeat { get; private set; }

    public TimeSpan HeartbeatPeriod { get; private set; }

    private void Start(bool onError = false)
    {
        if (_timer != null) return;

        var startDelay = onError ? LoopPeriodMs * 5 : LoopPeriodMs;
        _timer = new Timer(callback: LoopCallback, state: this, dueTime: startDelay, period: LoopPeriodMs);
        _id = Guid.NewGuid();
        State = GameLoopState.Running;
    }

    private void Stop()
    {
        if (_timer == null) return;
        _timer.Dispose();
        _timer = null;
        State = GameLoopState.Stopped;
    }

    private void StopWithError(Exception e)
    {
        _logger.LogError(e, e.Message);
        _timer?.Dispose();
        _timer = null;
        State = GameLoopState.Error;
    }

    private static void LoopCallback(object? state)
    {
        if (state is not GameLoop loop) return;

        try
        {
            RunLoop(loop);
        }
        catch (Exception e)
        {
            loop.StopWithError(e);
            loop.Start(true);
        }
    }

    private static void RunLoop(GameLoop loop)
    {
        loop.LastHeartbeat = loop.Heartbeat;
        loop.Heartbeat = DateTime.UtcNow;
        loop.HeartbeatPeriod = loop.Heartbeat - loop.LastHeartbeat;

        loop._logger.LogDebug("Game loop tick id:{id} games:{count}", loop._id, loop._state.Count);

        if (loop.State != GameLoopState.Running)
        {
            loop._logger.LogWarning("Loop is running while not in valid state: {state}", loop.State);
        }

        if (loop._state.Count < ParallelEntries)
        {
            foreach (var gameKv in loop._state)
            {
                RunLoopTick(loop, gameKv.Value);
            }
        }
        else
        {
            Parallel.ForEach(loop._state, gameKv => { RunLoopTick(loop, gameKv.Value); });
        }
    }

    private static void RunLoopTick(GameLoop loop, GameState game)
    {
        try
        {
            if (CanRemove(game))
            {
                if (game.State is GameStateEnum.Active)
                {
                    game.SetState(GameStateEnum.Canceled);
                }

                game.Dispose();
                var removed = loop._state.TryRemove(game.GameSessionId, out _);
                if (removed)
                {
                    loop._logger.LogDebug("Game loop removed gameId:{gameId}", game.GameSessionId);
                }
                else
                {
                    loop._logger.LogWarning("Game loop couldn't remove gameId:{gameId}", game.GameSessionId);
                }
            }
            else
            {
                RunGame(game);
            }
        }
        catch (Exception e)
        {
            loop._logger.LogError(e, e.Message);
        }
    }

    private static void RunGame(GameState game)
    {
        if (game is
            {
                State: GameStateEnum.Active or GameStateEnum.FinalCountdown,
                GameTimerStartedAt: not null, GameTimer: > 0
            })
        {
            game.RefreshGameTimer();
        }

        if (game is { State: GameStateEnum.Active } and
            ({ WinningPlayers.Count: > 0 } or { Preset.GameTimer: > 0, GameTimerStartedAt: not null, GameTimer: <= 0 }))
        {
            if (game.Preset.EndgameTimer > 0)
            {
                game.SetState(GameStateEnum.FinalCountdown);
                game.ResetGameTimer(game.Preset.EndgameTimer);
            }
            else
            {
                game.SetState(GameStateEnum.Finished);
            }
        }

        if (game is { State: GameStateEnum.FinalCountdown, GameTimerStartedAt: not null, GameTimer: <= 0 })
        {
            game.SetState(GameStateEnum.Finished);
        }

        if (game is
            {
                State: GameStateEnum.Finished or GameStateEnum.Canceled, WinningPlayers.Count: 0, Players.Count: > 0
            })
        {
            var maxScore = game.Players.Select(x => x.Score).Max();
            var playerIds = game.Players
                .Where(x => x.Score == maxScore)
                .Select(x => x.PlayerUserId).ToList();
            game.SetWinningPlayers(playerIds);
        }
    }

    private static bool CanRemove(GameState game)
    {
        var maxUpdated = game.Players.Select(x => x.UpdatedAt).Concat([game.UpdatedAt]).Max();
        var elapsed = DateTime.UtcNow - maxUpdated;
        return (game.State is GameStateEnum.Finished or GameStateEnum.Canceled
                && elapsed > TimeSpan.FromMinutes(RemoveFinishedAfterMin))
               || game.State is GameStateEnum.Active or GameStateEnum.Init
               && elapsed > TimeSpan.FromMinutes(RemoveInactiveAfterMin);
    }

    public void Dispose()
    {
        Stop();
    }
}

public enum GameLoopState
{
    Init,
    Running,
    Stopped,
    Error
}