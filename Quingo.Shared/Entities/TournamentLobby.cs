using Quingo.Shared.Constants;
using Quingo.Shared.Entities;

public class TournamentLobby : EntityBase
{
    public string HostUserId { get; set; } = default!;
    public string HostUserName { get; set; } = default!;
    public int PackId { get; set; }
    public string PackName { get; set; } = default!;
    public string? Password { get; set; }
    public string? PresetJson { get; set; }
    public TournamentMode TournamentMode { get; set; } = TournamentMode.None;
    public ICollection<LobbyParticipant> Participants { get; set; } = new List<LobbyParticipant>();
}