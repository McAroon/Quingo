namespace Quingo.Shared.Models;

public class LobbyParticipant
{
    public string UserId { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public bool IsReady { get; set; }
}