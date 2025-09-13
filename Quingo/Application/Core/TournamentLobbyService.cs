using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Quingo.Application.SignalR;
using Quingo.Infrastructure.Database;
using Quingo.Shared.Constants;
using Quingo.Shared.Entities;
using Quingo.Shared.Enums;
using System.Text.Json;

namespace Quingo.Application.Core;

public class TournamentLobbyService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IHubContext<LobbyHub> _hubContext;

    public TournamentLobbyService(IDbContextFactory<ApplicationDbContext> dbFactory, IHubContext<LobbyHub> hubContext)
    {
        _dbFactory = dbFactory;
        _hubContext = hubContext;
    }

    public async Task<TournamentLobby> CreateLobby(int packId, string packName, string hostId, string hostName, string? password, PackPresetData presetData, TournamentMode tournamentMode)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var lobby = new TournamentLobby
        {
            HostUserId = hostId,
            HostUserName = hostName,
            PackId = packId,
            PackName = packName,
            Password = password,
            PresetData = presetData,
            TournamentMode = tournamentMode,
            Participants = new List<LobbyParticipant>
            {
                new LobbyParticipant
                {
                    UserId = hostId,
                    UserName = hostName,
                    IsReady = false,
                    Order = tournamentMode == TournamentMode.QualificationAndPlayoff ? 0 : 1
                }
            }
        };

        db.TournamentLobbies.Add(lobby);
        await db.SaveChangesAsync();

        return lobby;
    }

    public async Task<List<TournamentLobby>> GetActiveLobbies()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.TournamentLobbies
            .AsNoTracking()
            .Include(x => x.Participants)
            .ToListAsync();
    }

    public async Task<TournamentLobby?> GetLobbyById(int lobbyId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        return await db.TournamentLobbies
            .AsNoTracking()
            .Include(x => x.Participants)
            .Include(x => x.Bans)
            .FirstOrDefaultAsync(x => x.Id == lobbyId);
    }

    public async Task<CanJoinLobbyResponse> GetJoinStatusAsync(int lobbyId, string userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var lobby = await db.TournamentLobbies
            .Include(x => x.Participants)
            .Include(x => x.Bans)
            .FirstOrDefaultAsync(x => x.Id == lobbyId);

        if (lobby is null)
            return new(CanJoinLobbyStatus.NotFound, "Лобби не найдено или было удалено.");

        if (lobby.Bans.Any(b => b.UserId == userId))
            return new(CanJoinLobbyStatus.Banned, "Вы забанены в этом лобби.");

        if (lobby.Participants.Any(p => p.UserId == userId))
            return new(CanJoinLobbyStatus.AlreadyIn);

        var anyResultsExist = await db.TournamentResults
            .AnyAsync(r => r.LobbyId == lobbyId);
        if (anyResultsExist)
            return new(CanJoinLobbyStatus.Started, "В лобби уже начались игры. Присоединение запрещено.");

        if (lobby.TournamentMode == TournamentMode.QualificationAndPlayoff)
        {
            if (lobby.Participants.Count >= 128)
                return new(CanJoinLobbyStatus.Full, "Достигнут лимит участников.");
        }
        else
        {
            var presetData = lobby.PresetData ?? new PackPresetData();
            if (presetData.MaxPlayers > 0 && lobby.Participants.Count >= presetData.MaxPlayers)
                return new(CanJoinLobbyStatus.Full, "Лобби уже заполнено.");
        }

        return new(CanJoinLobbyStatus.Ok);
    }


    public async Task JoinLobby(int lobbyId, string userId, string userName)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var lobby = await db.TournamentLobbies
            .Include(x => x.Participants)
            .Include(x => x.Bans)
            .FirstOrDefaultAsync(x => x.Id == lobbyId);

        if (lobby == null)
            throw new InvalidOperationException("Lobby not found");

        if (lobby.Bans.Any(b => b.UserId == userId))
            throw new InvalidOperationException("You are banned from this lobby");

        if (lobby.Participants.Any(p => p.UserId == userId))
            return;

        var newParticipant = new LobbyParticipant
        {
            UserId = userId,
            UserName = userName
        };
        lobby.Participants.Add(newParticipant);

        if (lobby.TournamentMode != TournamentMode.QualificationAndPlayoff)
        {
            var reordered = lobby.Participants
                .Where(p => p != newParticipant)
                .OrderBy(p => p.CreatedAt)
                .ThenBy(p => p.Order)
                .ToList();

            reordered.Add(newParticipant);

            for (int order = 1; order <= reordered.Count; order++)
                reordered[order - 1].Order = order;
        }

        if (lobby.TournamentMode == TournamentMode.QualificationAndPlayoff)
        {
            var preset = lobby.PresetData ?? new PackPresetData();
            var count = lobby.Participants.Count;
            var targetMax = Math.Max(3, count);

            if (preset.MaxPlayers != targetMax)
            {
                preset.MaxPlayers = targetMax;
                lobby.PresetData = preset;
            }
        }

        await db.SaveChangesAsync();

        await _hubContext.Clients.Group(SignalRConstants.LobbyGroup(lobbyId))
            .SendAsync(SignalRConstants.LobbyUpdated);
    }

    public async Task LeaveLobby(int lobbyId, string userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var hasResults = await db.TournamentResults
            .AnyAsync(r => r.LobbyId == lobbyId && r.UserId == userId);
        if (hasResults)
            return;

        var lobby = await db.TournamentLobbies
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.Id == lobbyId);

        if (lobby == null)
            return;

        var participant = lobby.Participants.FirstOrDefault(x => x.UserId == userId);
        if (participant != null)
            lobby.Participants.Remove(participant);

        if (lobby.TournamentMode == TournamentMode.QualificationAndPlayoff)
        {
            var reordered = lobby.Participants
                .OrderBy(p => p.CreatedAt)
                .ThenBy(p => p.Order)
                .ToList();

            for (int order = 1; order <= reordered.Count; order++)
                reordered[order - 1].Order = order;
        }

        if (lobby.TournamentMode == TournamentMode.QualificationAndPlayoff)
        {
            var preset = lobby.PresetData ?? new PackPresetData();
            var count = lobby.Participants.Count;
            var targetMax = Math.Max(3, count);

            if (preset.MaxPlayers != targetMax)
            {
                preset.MaxPlayers = targetMax;
                lobby.PresetData = preset;
            }
        }

        await db.SaveChangesAsync();
        await _hubContext.Clients.Group(SignalRConstants.LobbyGroup(lobbyId))
            .SendAsync(SignalRConstants.LobbyUpdated);
    }

    public async Task MarkReady(int lobbyId, string userId)
    {
        await UpdateReadyStatus(lobbyId, userId, true);
    }

    public async Task MarkNotReady(int lobbyId, string userId)
    {
        await UpdateReadyStatus(lobbyId, userId, false);
    }

    private async Task UpdateReadyStatus(int lobbyId, string userId, bool isReady)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var participant = await db.LobbyParticipants
            .FirstOrDefaultAsync(x => x.TournamentLobbyId == lobbyId && x.UserId == userId);

        if (participant is null)
            throw new Exception("Вы не в этом лобби");

        participant.IsReady = isReady;
        await db.SaveChangesAsync();

        await _hubContext.Clients.Group(SignalRConstants.LobbyGroup(lobbyId))
            .SendAsync(SignalRConstants.LobbyUpdated);
    }

    public async Task CloseLobby(int lobbyId, string hostUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var lobby = await db.TournamentLobbies
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.Id == lobbyId);

        if (lobby == null)
            throw new Exception("Лобби не найдено");

        if (lobby.HostUserId != hostUserId)
            throw new Exception("Только хост может закрыть лобби");

        db.LobbyParticipants.RemoveRange(lobby.Participants);
        db.TournamentLobbies.Remove(lobby);
        await db.SaveChangesAsync();

        await _hubContext.Clients.Group(SignalRConstants.LobbyGroup(lobbyId))
            .SendAsync(SignalRConstants.LobbyClosed);
    }

    public async Task UpdatePresetData(int lobbyId, PackPresetData data)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var lobby = await db.TournamentLobbies.FindAsync(lobbyId);
        if (lobby == null) throw new Exception("Lobby not found");

        lobby.PresetData = data;
        await db.SaveChangesAsync();

        await _hubContext.Clients.Group(SignalRConstants.LobbyGroup(lobbyId))
            .SendAsync(SignalRConstants.LobbyUpdated);
    }

    public async Task ShuffleParticipantsOrderAsync(int lobbyId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var anyResultsExist = await db.TournamentResults
            .AnyAsync(r => r.LobbyId == lobbyId);

        if (anyResultsExist)
            throw new InvalidOperationException("Нельзя перемешивать игроков после начала турнира.");

        var mode = await db.TournamentLobbies
            .Where(x => x.Id == lobbyId)
            .Select(x => x.TournamentMode)
            .FirstOrDefaultAsync();

        if (mode == TournamentMode.QualificationAndPlayoff)
            throw new InvalidOperationException("В режиме квалификации порядок не используется.");

        var participants = await db.LobbyParticipants
            .Where(p => p.TournamentLobbyId == lobbyId)
            .ToListAsync();

        var rnd = new Random();
        var shuffled = participants.OrderBy(x => rnd.Next()).ToList();

        for (int order = 1; order <= shuffled.Count; order++)
        {
            shuffled[order - 1].Order = order;
        }

        await db.SaveChangesAsync();

        await _hubContext.Clients.Group(SignalRConstants.LobbyGroup(lobbyId))
            .SendAsync(SignalRConstants.LobbyUpdated);
    }

    public async Task<int> RestartTournamentAsync(int lobbyId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var oldLobby = await db.TournamentLobbies
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.Id == lobbyId);

        if (oldLobby is null)
            throw new Exception("Лобби не найдено");

        var isQual = oldLobby.TournamentMode == TournamentMode.QualificationAndPlayoff;

        var newLobby = new TournamentLobby
        {
            HostUserId = oldLobby.HostUserId,
            HostUserName = oldLobby.HostUserName,
            PackId = oldLobby.PackId,
            PackName = oldLobby.PackName,
            Password = oldLobby.Password,
            PresetData = ClonePreset(oldLobby.PresetData),
            TournamentMode = oldLobby.TournamentMode,
            Participants = oldLobby.Participants.Select(p => new LobbyParticipant
            {
                UserId = p.UserId,
                UserName = p.UserName,
                IsReady = false,
                Order = isQual ? 0 : p.Order
            }).ToList()
        };

        db.TournamentLobbies.Add(newLobby);
        db.TournamentLobbies.Remove(oldLobby);
        await db.SaveChangesAsync();

        await _hubContext.Clients
            .Group(SignalRConstants.LobbyGroup(lobbyId))
            .SendAsync(SignalRConstants.LobbyRestarted, newLobby.Id);

        return newLobby.Id;
    }

    private static PackPresetData ClonePreset(PackPresetData src)
    {
        var json = JsonSerializer.Serialize(src);
        return JsonSerializer.Deserialize<PackPresetData>(json) ?? new PackPresetData();
    }

    public async Task KickAsync(int lobbyId, string hostUserId, string targetUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var lobby = await db.TournamentLobbies
            .Include(x => x.Participants)
            .Include(x => x.Bans)
            .FirstOrDefaultAsync(x => x.Id == lobbyId);

        if (lobby is null) throw new InvalidOperationException("Lobby not found");
        if (lobby.HostUserId != hostUserId) throw new InvalidOperationException("Only host can kick");
        if (targetUserId == hostUserId) throw new InvalidOperationException("Host cannot kick themselves");

        var participant = lobby.Participants.FirstOrDefault(p => p.UserId == targetUserId);
        if (participant is null) return;

        lobby.Participants.Remove(participant);

        if (lobby.TournamentMode == TournamentMode.QualificationAndPlayoff)
        {
            var reordered = lobby.Participants.OrderBy(p => p.CreatedAt).ThenBy(p => p.Order).ToList();
            for (int i = 0; i < reordered.Count; i++)
                reordered[i].Order = i + 1;

            var preset = lobby.PresetData ?? new PackPresetData();
            var count = lobby.Participants.Count;
            var targetMax = Math.Max(3, count);
            if (preset.MaxPlayers != targetMax)
            {
                preset.MaxPlayers = targetMax;
                lobby.PresetData = preset;
            }
        }

        await db.SaveChangesAsync();

        await _hubContext.Clients.Group(SignalRConstants.LobbyGroup(lobbyId))
            .SendAsync(SignalRConstants.LobbyUpdated);
    }

    public async Task BanAsync(int lobbyId, string hostUserId, string targetUserId, string targetUserName)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var lobby = await db.TournamentLobbies
            .Include(x => x.Participants)
            .Include(x => x.Bans)
            .FirstOrDefaultAsync(x => x.Id == lobbyId);

        if (lobby is null) throw new InvalidOperationException("Lobby not found");
        if (lobby.HostUserId != hostUserId) throw new InvalidOperationException("Only host can ban");
        if (targetUserId == hostUserId) throw new InvalidOperationException("Host cannot ban themselves");

        var participant = lobby.Participants.FirstOrDefault(p => p.UserId == targetUserId);
        if (participant is not null)
            lobby.Participants.Remove(participant);

        if (!lobby.Bans.Any(b => b.UserId == targetUserId))
        {
            lobby.Bans.Add(new LobbyBan
            {
                TournamentLobbyId = lobby.Id,
                UserId = targetUserId,
                UserName = targetUserName
            });
        }

        if (lobby.TournamentMode == TournamentMode.QualificationAndPlayoff)
        {
            var preset = lobby.PresetData ?? new PackPresetData();
            var count = lobby.Participants.Count;
            var targetMax = Math.Max(3, count);
            if (preset.MaxPlayers != targetMax)
            {
                preset.MaxPlayers = targetMax;
                lobby.PresetData = preset;
            }
        }

        try { await db.SaveChangesAsync(); }
        catch (Exception ex)
        {

        }
        

        await _hubContext.Clients.Group(SignalRConstants.LobbyGroup(lobbyId))
            .SendAsync(SignalRConstants.LobbyUpdated);
    }

    public async Task UnbanAsync(int lobbyId, string hostUserId, string targetUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var lobby = await db.TournamentLobbies
            .Include(x => x.Bans)
            .FirstOrDefaultAsync(x => x.Id == lobbyId);

        if (lobby is null) throw new InvalidOperationException("Lobby not found");
        if (lobby.HostUserId != hostUserId) throw new InvalidOperationException("Only host can unban");

        var ban = lobby.Bans.FirstOrDefault(b => b.UserId == targetUserId);
        if (ban is null) return;

        db.LobbyBans.Remove(ban);

        await db.SaveChangesAsync();
        await _hubContext.Clients.Group(SignalRConstants.LobbyGroup(lobbyId))
            .SendAsync(SignalRConstants.LobbyUpdated);
    }
}