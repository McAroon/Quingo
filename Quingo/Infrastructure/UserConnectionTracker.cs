using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Quingo.Infrastructure;

public class UserConnectionTracker(ILogger<UserConnectionTracker> logger) : IDisposable
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> _userConnections = [];

    public void Connected(string userId, string circuitId)
    {
        _userConnections.AddOrUpdate(userId, _ => new ConcurrentDictionary<string, bool> { [circuitId] = true },
            (_, circuits) =>
            {
                circuits[circuitId] = true;
                return circuits;
            });
        logger.LogDebug("Connected {userId} {circuitId}", userId, circuitId);
        ConnectionsChanged?.Invoke(userId, true);
    }

    public void Disconnected(string userId, string circuitId)
    {
        _userConnections.AddOrUpdate(userId, _ => new ConcurrentDictionary<string, bool> { [circuitId] = false },
            (_, circuits) =>
            {
                {
                    circuits[circuitId] = false;
                    return circuits;
                }
            });
        logger.LogDebug("Disconnected {userId} {circuitId}", userId, circuitId);
        ConnectionsChanged?.Invoke(userId, false);
    }

    public bool IsConnected(string userId) =>
        _userConnections.TryGetValue(userId, out var circuits) && circuits.Any(x => x.Value);

    public event Action<string, bool>? ConnectionsChanged;

    public void Dispose()
    {
        ConnectionsChanged = null;
    }
}

public class TrackingCircuitHandler(
    AuthenticationStateProvider authStateProvider,
    UserConnectionTracker tracker)
    : CircuitHandler
{
    public override async Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var userId = await GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        tracker.Connected(userId, circuit.Id);
    }

    public override async Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var userId = await GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        tracker.Disconnected(userId, circuit.Id);
    }

    private async Task<string?> GetUserId()
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var userId = authState?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId;
    }
}