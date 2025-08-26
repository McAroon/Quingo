using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Quingo.Application.SignalR;
using Quingo.Infrastructure.Database;
using Quingo.Shared.Constants;
using Quingo.Shared.Entities;

namespace Quingo.Application.Core;

public class PlayoffService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IHubContext<LobbyHub> _hubContext;

    public PlayoffService(IDbContextFactory<ApplicationDbContext> dbFactory, IHubContext<LobbyHub> hubContext)
    {
        _dbFactory = dbFactory;
        _hubContext = hubContext;
    }

    public async Task CreateInitialTournamentResultsAsync(int lobbyId, Guid gameSessionId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var games = await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId)
            .Select(r => r.Game)
            .ToListAsync();

        var maxGame = games.DefaultIfEmpty(0).Max();
        var nextGame = maxGame + 1;

        var existingResults = await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId && (r.Result == null || r.Result == GameResult.Draw))
            .ToListAsync();

        if (existingResults.Any())
        {
            foreach (var result in existingResults)
                result.GameSessionId = gameSessionId;

            await db.SaveChangesAsync();

            await _hubContext.Clients.Group(SignalRConstants.LobbyGroup(lobbyId))
                .SendAsync(SignalRConstants.LobbyUpdated);

            return;
        }

        var players = await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId && r.Game == maxGame && r.Result == GameResult.Win)
            .OrderBy(r => r.Id)
            .ToListAsync();

        if (!players.Any())
        {
            var lobby = await db.TournamentLobbies
                .Include(l => l.Participants)
                .FirstOrDefaultAsync(l => l.Id == lobbyId);

            if (lobby is null) return;

            players = lobby.Participants
                .OrderBy(p => p.Order)
                .Select(p => new TournamentResult
                {
                    LobbyId = lobbyId,
                    UserId = p.UserId,
                    UserName = p.UserName
                })
                .ToList();
        }

        var hasThirdPlaceGame = await db.TournamentResults
            .AnyAsync(r => r.LobbyId == lobbyId && r.Game == 0);

        if (!hasThirdPlaceGame)
        {
            var losers = await db.TournamentResults
                .Where(r => r.LobbyId == lobbyId && r.Game == maxGame && r.Result == GameResult.Loss)
                .ToListAsync();

            if (losers.Count == 2)
                players = losers;
        }

        if (players.Count < 2)
            return;

        var results = new List<TournamentResult>();

        for (int i = 0; i < players.Count; i += 2)
        {
            var p1 = players.ElementAtOrDefault(i);
            var p2 = players.ElementAtOrDefault(i + 1);

            if (p1 == null || p2 == null)
                continue;

            results.AddRange([
                new TournamentResult
            {
                LobbyId = lobbyId,
                GameSessionId = gameSessionId,
                UserId = p1.UserId,
                UserName = p1.UserName,
                Score = 0,
                Game = !hasThirdPlaceGame && players.Count == 2 ? 0 : nextGame
            },
            new TournamentResult
            {
                LobbyId = lobbyId,
                GameSessionId = gameSessionId,
                UserId = p2.UserId,
                UserName = p2.UserName,
                Score = 0,
                Game = !hasThirdPlaceGame && players.Count == 2 ? 0 : nextGame
            }
            ]);
        }

        db.TournamentResults.AddRange(results);

        await db.SaveChangesAsync();

        await _hubContext.Clients.Group(SignalRConstants.LobbyGroup(lobbyId))
            .SendAsync(SignalRConstants.LobbyUpdated);
    }

    public async Task SaveTournamentResultAsync(int lobbyId, Guid gameId, IEnumerable<PlayerInstance> players)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var lobby = await db.TournamentLobbies
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.Id == lobbyId);

        if (lobby is null) return;

        var participantMap = lobby.Participants.ToDictionary(p => p.UserId, p => p);

        var orderedPlayers = players
            .Where(p => participantMap.ContainsKey(p.PlayerUserId))
            .OrderBy(p => participantMap[p.PlayerUserId].Order)
            .ToList();

        var existingResults = await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId
                        && r.Game == 0
                        && r.GameSessionId == gameId)
            .OrderBy(r => r.Id)
            .ToListAsync();

        if (existingResults.Count == 0)
        {
            existingResults = await db.TournamentResults
                .Where(r => r.LobbyId == lobbyId && r.GameSessionId == gameId)
                .OrderBy(r => r.Id)
                .ToListAsync();
        }

        if (existingResults.Count == 0)
        {
            var allGames = await db.TournamentResults
                .Where(r => r.LobbyId == lobbyId)
                .Select(r => r.Game)
                .ToListAsync();

            var currentGame = allGames.DefaultIfEmpty(0).Max();

            existingResults = await db.TournamentResults
                .Where(r => r.LobbyId == lobbyId && r.Game == currentGame)
                .OrderBy(r => r.Id)
                .ToListAsync();

            if (currentGame == 0 && existingResults.Count > 0 && existingResults.Any(r => r.GameSessionId == Guid.Empty))
            {
                foreach (var tr in existingResults)
                    tr.GameSessionId = gameId;
            }
        }

        if (existingResults.Count == 0)
            return;

        foreach (var player in orderedPlayers)
        {
            var result = existingResults.FirstOrDefault(r => r.UserId == player.PlayerUserId);
            if (result != null)
            {
                result.Score = player.Score.ScoreTotal;
                result.CellScore = player.Score.ScoreCells;
                result.ErrorPenalty = player.Score.ScoreErrorPenalties;
                result.DrawHistory = player.DrawState?.DrawnNodes?.Count ?? 0;
                result.TimeBonus = player.Score.ScoreRemainingTime;
                result.Result = null;
                result.GameSessionId = gameId;
            }
        }

        for (int i = 0; i < orderedPlayers.Count; i += 2)
        {
            var p1 = orderedPlayers[i];
            var p2 = (i + 1 < orderedPlayers.Count) ? orderedPlayers[i + 1] : null;
            if (p2 is null) continue;

            var r1 = existingResults.FirstOrDefault(r => r.UserId == p1.PlayerUserId);
            var r2 = existingResults.FirstOrDefault(r => r.UserId == p2.PlayerUserId);
            if (r1 == null || r2 == null) continue;

            DetermineWinner(r1, r2);
        }

        foreach (var participant in lobby.Participants)
            participant.IsReady = false;

        await db.SaveChangesAsync();

        await _hubContext.Clients
            .Group($"lobby-{lobbyId}")
            .SendAsync("TournamentUpdated");
    }

    public async Task<Dictionary<int, List<TournamentResult>>> GetTournamentHistoryAsync(int lobbyId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        return await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId && r.Result != null)
            .OrderBy(r => r.Game)
            .ThenBy(r => r.Id)
            .GroupBy(r => r.Game)
            .ToDictionaryAsync(g => g.Key, g => g.ToList());
    }

    public async Task<Guid?> GetSessionIdAsync(int lobbyId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var sessionId = await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId && (r.Result == null || r.Result == GameResult.Draw))
            .OrderByDescending(r => r.Game)
            .Select(r => (Guid?)r.GameSessionId)
            .FirstOrDefaultAsync();

        return sessionId;
    }

    public async Task<List<TournamentResult>> GetWinnersOfLastRoundAsync(int lobbyId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var gameNumbers = await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId && r.Result != null)
            .Select(r => r.Game)
            .ToListAsync();

        var maxGame = gameNumbers.DefaultIfEmpty(0).Max();

        if (maxGame == 0)
            return new List<TournamentResult>();

        return await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId && r.Game == maxGame && r.Result == GameResult.Win)
            .ToListAsync();
    }

    public async Task<List<TournamentResult>> GetDrawPlayersOfLastRoundAsync(int lobbyId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var gameNumbers = await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId && r.Result != null)
            .Select(r => r.Game)
            .ToListAsync();

        var maxGame = gameNumbers.DefaultIfEmpty(0).Max();

        if (maxGame == 0)
            return new List<TournamentResult>();

        return await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId && r.Game == maxGame && r.Result == GameResult.Draw)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetLatestScoresAsync(int lobbyId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        return await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId)
            .GroupBy(r => r.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                LatestScore = g.OrderByDescending(r => r.Id).First().Score
            })
            .ToDictionaryAsync(x => x.UserId, x => x.LatestScore);
    }

    public async Task<List<TournamentResult>> GetThirdPlaceCandidatesAsync(int lobbyId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var gameNumbers = await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId && r.Result != null)
            .Select(r => r.Game)
            .ToListAsync();

        var maxGame = gameNumbers.DefaultIfEmpty(0).Max();

        var lastResults = await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId && r.Game == maxGame)
            .ToListAsync();

        if (lastResults.Count != 4)
            return new();

        var hasNextGame = await db.TournamentResults
            .AnyAsync(r => r.LobbyId == lobbyId && r.Game == maxGame + 1);

        if (hasNextGame)
            return new();

        var losers = lastResults
            .Where(r => r.Result == GameResult.Loss)
            .ToList();

        return losers.Count == 2 ? losers : new();
    }

    private void DetermineWinner(TournamentResult r1, TournamentResult r2)
    {
        var comparisons = new List<(int v1, int v2, Action win1, Action win2)>
    {
        (r1.Score,        r2.Score,        () => SetWin(r1, r2), () => SetWin(r2, r1)),
        (r1.CellScore,    r2.CellScore,    () => SetWin(r1, r2), () => SetWin(r2, r1)),
        (r2.ErrorPenalty, r1.ErrorPenalty, () => SetWin(r1, r2), () => SetWin(r2, r1)),
        (r2.DrawHistory,  r1.DrawHistory,  () => SetWin(r1, r2), () => SetWin(r2, r1))
    };

        foreach (var (v1, v2, win1, win2) in comparisons)
        {
            if (v1 > v2) { win1(); return; }
            if (v1 < v2) { win2(); return; }
        }

        r1.Result = GameResult.Draw;
        r2.Result = GameResult.Draw;
    }

    private void SetWin(TournamentResult winner, TournamentResult loser)
    {
        winner.Result = GameResult.Win;
        loser.Result = GameResult.Loss;
    }
}