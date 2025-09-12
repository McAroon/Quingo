namespace Quingo.Shared.Entities;

public class LobbyBan : EntityBase
{
    public int TournamentLobbyId { get; set; }
    public string UserId { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public TournamentLobby Lobby { get; set; } = default!;
}