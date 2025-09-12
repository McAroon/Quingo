using Quingo.Shared.Enums;

namespace Quingo.Shared.Entities;

public class UserPackPreset : EntityBase
{
    public string UserId { get; set; } = null!;
    public int PackId { get; set; }
    public TournamentMode TournamentMode { get; set; } = TournamentMode.None;
    public PackPresetData Data { get; set; } = new();
}