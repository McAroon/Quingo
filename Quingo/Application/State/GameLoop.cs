namespace Quingo.Application.State;

public class GameLoop : IDisposable
{
    private readonly ILogger _logger;
    private readonly GameState _game;
    private Timer? _timer;

    public GameLoop(ILogger logger, GameState game)
    {
        _logger = logger;
        _game = game;
        Start();
    }

    public GameLoopState State { get; private set; }

    private void Start()
    {
        if (_timer != null) return;
        _timer = new Timer(callback: LoopCallback, state: this, dueTime: 1000, period: 1000);
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
        }
    }

    private static void RunLoop(GameLoop loop)
    {
        loop._logger.LogDebug("Loop tick {id}", loop._game.GameSessionId);

        if (loop.State != GameLoopState.Running)
        {
            loop._logger.LogWarning("Loop is running while not in valid state: {state}", loop.State);
        }

        if (loop._game.State is GameStateEnum.Active or GameStateEnum.FinalCountdown)
        {
            if (loop._game.GameTimer > 0)
            {
                if (loop._game.IsGameTimerRunning)
                {
                    loop._game.DecrementGameTimer();
                }
                
                if (loop._game.State is GameStateEnum.Active && loop._game.WinningPlayers.Count > 0)
                {
                    if (loop._game.Preset.EndgameTimer > 0 && loop._game.GameTimer > loop._game.Preset.EndgameTimer)
                    {
                        loop._game.SetState(GameStateEnum.FinalCountdown);
                        loop._game.SetGameTimer(loop._game.Preset.EndgameTimer);
                    }
                }
            }
            else if (loop._game.Preset.GameTimer > 0 || loop._game.State is GameStateEnum.FinalCountdown)
            {
                if (loop._game.State is GameStateEnum.Active)
                {
                    loop._game.SetState(GameStateEnum.FinalCountdown);
                    loop._game.SetGameTimer(loop._game.Preset.EndgameTimer);
                }
                else
                {
                    loop._game.SetState(GameStateEnum.Finished);
                }
            }
        }

        if (loop._game.State is GameStateEnum.Finished or GameStateEnum.Canceled &&
            loop._game.WinningPlayers.Count == 0 && loop._game.Players.Count > 0)
        {
            var maxScore = loop._game.Players.Select(x => x.Score).Max();
            var playerIds = loop._game.Players
                .Where(x => x.Score == maxScore)
                .Select(x => x.PlayerUserId).ToList();
            loop._game.SetWinningPlayers(playerIds);
        }
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