using Quingo.Shared.Constants;

namespace Quingo.Shared.Entities;

public class TournamentResult : EntityBase
{
    public int LobbyId { get; set; }
    public Guid GameSessionId { get; set; }
    public string UserId { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public int Score { get; set; }
    public int CellScore { get; set; }
    public int TimeBonus { get; set; }
    public int ErrorPenalty { get; set; }
    public int DrawHistory { get; set; }
    public int Game { get; set; } = 0;
    public int Position { get; set; } = 0;
    public bool IsRandom { get; set; } = false;
    public GameResult? Result { get; set; } = null!;
}