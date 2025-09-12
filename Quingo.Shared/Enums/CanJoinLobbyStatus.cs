namespace Quingo.Shared.Enums;

public enum CanJoinLobbyStatus
{
    Ok,
    AlreadyIn,
    Banned,
    Full,
    Started,
    NotFound,
    Error
}

public record CanJoinLobbyResponse(
    CanJoinLobbyStatus Status,
    string? Message = null
);