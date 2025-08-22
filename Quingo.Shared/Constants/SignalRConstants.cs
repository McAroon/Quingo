namespace Quingo.Shared.Constants;

public class SignalRConstants
{
    public static string LobbyGroup(int lobbyId) => $"lobby-{lobbyId}";
    public const string LobbyHubPath = "/hubs/lobby";
    public const string JoinLobbyGroup = "JoinLobbyGroup";
    public const string LeaveLobbyGroup = "LeaveLobbyGroup";
    public const string LobbyUpdated = "LobbyUpdated";
    public const string LobbyClosed = "LobbyClosed";
}