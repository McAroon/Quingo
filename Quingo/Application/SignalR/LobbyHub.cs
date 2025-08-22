using Microsoft.AspNetCore.SignalR;
using Quingo.Shared.Constants;

namespace Quingo.Application.SignalR;

public class LobbyHub : Hub
{
    public async Task JoinLobbyGroup(int lobbyId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, SignalRConstants.LobbyGroup(lobbyId));
    }

    public async Task LeaveLobbyGroup(int lobbyId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, SignalRConstants.LobbyGroup(lobbyId));
    }

    public async Task NotifyGameStarted(int lobbyId, Guid gameSessionId, string password)
    {
        var groupName = SignalRConstants.LobbyGroup(lobbyId);
        await Clients.Group(groupName).SendAsync("GameStarted", gameSessionId, password);
    }

    public async Task NotifyTournamentUpdated(int lobbyId)
    {
        var groupName = SignalRConstants.LobbyGroup(lobbyId);
        await Clients.Group(groupName).SendAsync("TournamentUpdated");
    }
}