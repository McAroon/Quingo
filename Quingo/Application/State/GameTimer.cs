using System.Diagnostics;

namespace Quingo.Application.State;

public class GameTimer(int initialValue)
{
    private readonly Stopwatch _stopwatch = new();
    public bool IsRunning => _stopwatch.IsRunning;

    private int _initialValue = initialValue;

    public int Value => _initialValue <= 0 ? 0 : (int)Math.Round(_initialValue - _stopwatch.Elapsed.TotalSeconds);

    public string DisplayValue => TimeSpan.FromSeconds(Value > 0 ? Value : 0).ToStringHoursOptional();

    public GameTimer Start()
    {
        if (_initialValue > 0)
            _stopwatch.Start();
        return this;
    }

    public void Stop()
    {
        _stopwatch.Stop();
    }

    public GameTimer Reset(int initialValue)
    {
        _initialValue = initialValue;
        _stopwatch.Reset();
        return this;
    }
}