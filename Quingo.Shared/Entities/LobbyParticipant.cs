using Quingo.Shared.Entities;

public class LobbyParticipant : EntityBase
{
    public int TournamentLobbyId { get; set; }
    public TournamentLobby Lobby { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public bool IsReady { get; set; } = false;
    public int Order { get; set; }
}