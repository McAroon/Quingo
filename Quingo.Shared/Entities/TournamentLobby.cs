using Quingo.Shared.Entities;
using Quingo.Shared.Enums;

public class TournamentLobby : EntityBase
{
    public string HostUserId { get; set; } = default!;
    public string HostUserName { get; set; } = default!;
    public int PackId { get; set; }
    public string PackName { get; set; } = default!;
    public string? Password { get; set; }
    public PackPresetData PresetData { get; set; } = new();
    public TournamentMode TournamentMode { get; set; } = TournamentMode.None;
    public ICollection<LobbyParticipant> Participants { get; set; } = new List<LobbyParticipant>();
    public ICollection<LobbyBan> Bans { get; set; } = new List<LobbyBan>();
}