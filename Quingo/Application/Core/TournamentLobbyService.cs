using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Quingo.Application.Host.Pages;
using Quingo.Application.SignalR;
using Quingo.Infrastructure.Database;
using Quingo.Shared.Constants;
using Quingo.Shared.Entities;
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
            PresetJson = JsonSerializer.Serialize(presetData),
            TournamentMode = tournamentMode,
            Participants = new List<LobbyParticipant>
            {
                new LobbyParticipant
                {
                    UserId = hostId,
                    UserName = hostName,
                    IsReady = false
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
            .Include(x => x.Participants)
            .ToListAsync();
    }

    public async Task<TournamentLobby?> GetLobbyById(int lobbyId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.TournamentLobbies
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.Id == lobbyId);
    }

    public async Task<bool> CanJoinLobbyAsync(int lobbyId, string userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var lobby = await db.TournamentLobbies
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.Id == lobbyId);

        if (lobby == null)
            return false;

        var presetData = JsonSerializer.Deserialize<PackPresetData>(lobby.PresetJson);

        if (lobby.Participants.Any(p => p.UserId == userId))
            return true;

        if (lobby.Participants.Count >= presetData.MaxPlayers)
            return false;

        return true;
    }

    public async Task JoinLobby(int lobbyId, string userId, string userName)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var lobby = await db.TournamentLobbies
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.Id == lobbyId);

        if (lobby == null)
            throw new InvalidOperationException("Lobby not found");

        if (lobby.Participants.Any(p => p.UserId == userId))
            return;

        lobby.Participants.Add(new LobbyParticipant
        {
            UserId = userId,
            UserName = userName
        });

        var reordered = lobby.Participants
            .OrderBy(p => p.CreatedAt)
            .ToList();

        for (int i = 0; i < reordered.Count; i++)
        {
            reordered[i].Order = i;
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

        var participant = await db.LobbyParticipants
            .FirstOrDefaultAsync(x => x.TournamentLobbyId == lobbyId && x.UserId == userId);

        if (participant != null)
        {
            db.LobbyParticipants.Remove(participant);
            await db.SaveChangesAsync();
            await _hubContext.Clients.Group(SignalRConstants.LobbyGroup(lobbyId))
                .SendAsync(SignalRConstants.LobbyUpdated);
        }
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

        lobby.PresetJson = JsonSerializer.Serialize(data);
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

        var participants = await db.LobbyParticipants
            .Where(p => p.TournamentLobbyId == lobbyId)
            .ToListAsync();

        var rnd = new Random();
        var shuffled = participants.OrderBy(x => rnd.Next()).ToList();

        for (int i = 0; i < shuffled.Count; i++)
        {
            shuffled[i].Order = i;
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

        var newLobby = new TournamentLobby
        {
            HostUserId = oldLobby.HostUserId,
            HostUserName = oldLobby.HostUserName,
            PackId = oldLobby.PackId,
            PackName = oldLobby.PackName,
            Password = oldLobby.Password,
            PresetJson = oldLobby.PresetJson,
            TournamentMode = oldLobby.TournamentMode,
            Participants = oldLobby.Participants.Select(p => new LobbyParticipant
            {
                UserId = p.UserId,
                UserName = p.UserName,
                IsReady = false,
                Order = p.Order
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
}