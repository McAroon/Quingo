using System.Diagnostics;

namespace Quingo.Application.State;

public class GameTimer(int initialValue)
{
    private readonly Stopwatch _stopwatch = new();
    public bool IsRunning => _stopwatch.IsRunning && Value > 0;

    public int Value
    {
        get
        {
            var value = (int)Math.Round(initialValue - _stopwatch.Elapsed.TotalSeconds);
            return value > 0 ? value : 0;
        }
    }

    public GameTimer Start()
    {
        _stopwatch.Start();
        return this;
    }

    public GameTimer Stop()
    {
        _stopwatch.Stop();
        return this;
    }
}