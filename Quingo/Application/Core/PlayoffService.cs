using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Quingo.Application.SignalR;
using Quingo.Infrastructure.Database;
using Quingo.Shared.Constants;
using Quingo.Shared.Entities;
using Quingo.Shared.Enums;

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

        var maxGame = await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId)
            .Select(r => (int?)r.Game)
            .MaxAsync() ?? 0;

        var maxPositiveGame = await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId && r.Game > 0)
            .Select(r => (int?)r.Game)
            .MaxAsync() ?? 0;

        var nextGame = maxPositiveGame + 1;

        var lastPositiveGame = await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId && r.Game > 0)
            .Select(r => (int?)r.Game)
            .MaxAsync() ?? 0;

        var openExists = await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId
                     && (r.Game == lastPositiveGame || r.Game == 0)
                     && (r.Result == null || r.Result == GameResult.Draw))
            .AnyAsync();

        if (openExists)
        {
            var toAttach = await db.TournamentResults
                .Where(r => r.LobbyId == lobbyId
                         && (r.Game == lastPositiveGame || r.Game == 0)
                         && (r.Result == null || r.Result == GameResult.Draw))
                .ToListAsync();

            foreach (var r in toAttach)
                r.GameSessionId = gameSessionId;

            await db.SaveChangesAsync();
            await _hubContext.Clients.Group(SignalRConstants.LobbyGroup(lobbyId))
                .SendAsync(SignalRConstants.LobbyUpdated);
            return;
        }

        var lobby = await db.TournamentLobbies
            .AsNoTracking()
            .Include(l => l.Participants)
            .FirstOrDefaultAsync(l => l.Id == lobbyId);
        if (lobby is null) return;

        var orderMap = lobby.Participants.ToDictionary(p => p.UserId, p => p.Order);

        var isQualificationPhaseStart = lobby.TournamentMode == TournamentMode.QualificationAndPlayoff
            && !await db.TournamentResults.AsNoTracking()
                .AnyAsync(r => r.LobbyId == lobbyId && r.Game == -1);

        List<TournamentResult> finalists = [];
        List<TournamentResult> bronzePair = [];
        List<TournamentResult> players = [];

        if (isQualificationPhaseStart)
        {
            players = lobby.Participants
                .OrderBy(p => p.CreatedAt)
                .Select(p => new TournamentResult
                {
                    LobbyId = lobbyId,
                    UserId = p.UserId,
                    UserName = p.UserName
                })
                .ToList();
        }
        else
        {
            if (maxPositiveGame > 0)
            {
                finalists = await db.TournamentResults.AsNoTracking()
                    .Where(r => r.LobbyId == lobbyId && r.Game == maxPositiveGame && r.Result == GameResult.Win)
                    .OrderBy(r => r.Id)
                    .ToListAsync();

                bronzePair = await db.TournamentResults.AsNoTracking()
                    .Where(r => r.LobbyId == lobbyId && r.Game == maxPositiveGame && r.Result == GameResult.Loss)
                    .OrderBy(r => r.Id)
                    .ToListAsync();
            }

            if (!finalists.Any())
            {
                if (lobby.TournamentMode == TournamentMode.QualificationAndPlayoff)
                {
                    players = lobby.Participants
                        .Where(p => p.Order > 0)
                        .OrderBy(p => p.Order)
                        .Select(p => new TournamentResult
                        {
                            LobbyId = lobbyId,
                            UserId = p.UserId,
                            UserName = p.UserName
                        })
                        .ToList();
                }
                else
                {
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
            }
        }

        var toInsert = new List<TournamentResult>(players.Count);

        if (isQualificationPhaseStart)
        {
            foreach (var p in players)
            {
                toInsert.Add(new TournamentResult
                {
                    LobbyId = lobbyId,
                    GameSessionId = gameSessionId,
                    UserId = p.UserId,
                    UserName = p.UserName,
                    Score = 0,
                    Game = -1
                });
            }
        }
        else
        {
            var hasThirdPlaceGame = await db.TournamentResults.AsNoTracking()
                   .AnyAsync(r => r.LobbyId == lobbyId && r.Game == 0);

            if (finalists.Count == 2 && bronzePair.Count == 2 && !hasThirdPlaceGame)
            {
                toInsert.Add(new TournamentResult
                {
                    LobbyId = lobbyId,
                    GameSessionId = gameSessionId,
                    UserId = finalists[0].UserId,
                    UserName = finalists[0].UserName,
                    Score = 0,
                    Game = nextGame
                });
                toInsert.Add(new TournamentResult
                {
                    LobbyId = lobbyId,
                    GameSessionId = gameSessionId,
                    UserId = finalists[1].UserId,
                    UserName = finalists[1].UserName,
                    Score = 0,
                    Game = nextGame
                });

                toInsert.Add(new TournamentResult
                {
                    LobbyId = lobbyId,
                    GameSessionId = gameSessionId,
                    UserId = bronzePair[0].UserId,
                    UserName = bronzePair[0].UserName,
                    Score = 0,
                    Game = 0
                });
                toInsert.Add(new TournamentResult
                {
                    LobbyId = lobbyId,
                    GameSessionId = gameSessionId,
                    UserId = bronzePair[1].UserId,
                    UserName = bronzePair[1].UserName,
                    Score = 0,
                    Game = 0
                });
            }
            else
            {
                for (int i = 0; i < players.Count; i += 2)
                {
                    var p1 = players.ElementAtOrDefault(i);
                    var p2 = players.ElementAtOrDefault(i + 1);
                    if (p1 == null || p2 == null) continue;

                    toInsert.Add(new TournamentResult
                    {
                        LobbyId = lobbyId,
                        GameSessionId = gameSessionId,
                        UserId = p1.UserId,
                        UserName = p1.UserName,
                        Score = 0,
                        Game = nextGame
                    });
                    toInsert.Add(new TournamentResult
                    {
                        LobbyId = lobbyId,
                        GameSessionId = gameSessionId,
                        UserId = p2.UserId,
                        UserName = p2.UserName,
                        Score = 0,
                        Game = nextGame
                    });
                }
            }
        }

        if (toInsert.Count == 0) return;

        db.TournamentResults.AddRange(toInsert);
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

        var existingResults = await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId && r.GameSessionId == gameId)
            .OrderBy(r => r.Game)
            .ThenBy(r => r.Id)
            .ToListAsync();

        if (existingResults.Count == 0)
        {
            var currentGame = await db.TournamentResults
                .Where(r => r.LobbyId == lobbyId)
                .Select(r => (int?)r.Game)
                .MaxAsync() ?? 0;

            existingResults = await db.TournamentResults
                .Where(r => r.LobbyId == lobbyId && r.Game == currentGame)
                .OrderBy(r => r.Game)
                .ThenBy(r => r.Id)
                .ToListAsync();

            if (existingResults.Count > 0 && existingResults.Any(r => r.GameSessionId == Guid.Empty))
            {
                foreach (var tr in existingResults)
                    tr.GameSessionId = gameId;
            }
        }

        if (existingResults.Count == 0)
            return;

        var statsByUser = players.ToDictionary(p => p.PlayerUserId, p => p);

        foreach (var r in existingResults)
        {
            if (statsByUser.TryGetValue(r.UserId, out var pl))
            {
                r.Score = pl.Score.ScoreTotal;
                r.CellScore = pl.Score.ScoreCells;
                r.ErrorPenalty = pl.Score.ScoreErrorPenalties;
                r.DrawHistory = pl.DrawState?.DrawnNodes?.Count ?? 0;
                r.TimeBonus = pl.Score.ScoreRemainingTime;
                r.Result = null;
                r.GameSessionId = gameId;
            }
        }

        var groupsByGame = existingResults
            .GroupBy(r => r.Game)
            .OrderBy(g => g.Key);

        foreach (var group in groupsByGame)
        {
            var list = group.OrderBy(r => r.Id).ToList();
            for (int i = 0; i + 1 < list.Count; i += 2)
            {
                var r1 = list[i];
                var r2 = list[i + 1];
                DetermineWinner(r1, r2);
            }
        }

        foreach (var participant in lobby.Participants)
            participant.IsReady = false;

        bool isQualificationRound =
            lobby.TournamentMode == TournamentMode.QualificationAndPlayoff &&
            existingResults.Any(r => r.Game == -1);

        if (isQualificationRound)
        {
            var perUser = existingResults
                .Where(r => r.Game == -1)
                .GroupBy(r => r.UserId)
                .Select(g => g.First())
                .ToList();

            var rnd = new Random(lobbyId);
            var randKey = perUser.ToDictionary(r => r.UserId, _ => rnd.Next());

            var ranked = perUser
                .OrderByDescending(r => r.Score)
                .ThenByDescending(r => r.CellScore)
                .ThenBy(r => r.ErrorPenalty)
                .ThenBy(r => r.DrawHistory)
                .ThenBy(r => randKey[r.UserId])
                .ToList();

            int n = ranked.Count;
            int k = HighestPowerOfTwoLE(n);

            var qualifiedIds = new HashSet<string>(ranked.Take(k).Select(r => r.UserId));

            if (k >= 2)
            {
                var topK = ranked.Take(k).ToList();

                var bracketOrderUserIds = new List<string>(k);
                for (int i = 0; i < k / 2; i++)
                {
                    bracketOrderUserIds.Add(topK[i].UserId);
                    bracketOrderUserIds.Add(topK[k - 1 - i].UserId);
                }

                int order = 1;
                foreach (var userId in bracketOrderUserIds)
                    if (participantMap.TryGetValue(userId, out var lp))
                        lp.Order = order++;

                foreach (var lp in lobby.Participants)
                    if (!qualifiedIds.Contains(lp.UserId))
                        lp.Order = 0;

                foreach (var tr in existingResults)
                    tr.Result = qualifiedIds.Contains(tr.UserId) ? GameResult.Win : GameResult.Loss;
            }
            else
            {
                foreach (var lp in lobby.Participants)
                    lp.Order = 0;

                qualifiedIds.Clear();
            }

            var randomIds = perUser
                .GroupBy(r => new { r.Score, r.CellScore, r.ErrorPenalty, r.DrawHistory })
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.Select(x => x.UserId))
                .ToHashSet();

            foreach (var tr in existingResults.Where(x => x.Game == -1))
                tr.IsRandom = randomIds.Contains(tr.UserId);

            for (int i = 0; i < ranked.Count; i++)
            {
                var userId = ranked[i].UserId;
                int position = qualifiedIds.Contains(userId) ? (i + 1) : 0;

                foreach (var tr in existingResults.Where(x => x.UserId == userId && x.Game == -1))
                    tr.Position = position;
            }

            foreach (var tr in existingResults.Where(x => x.Game == -1))
                tr.Result = qualifiedIds.Contains(tr.UserId) ? GameResult.Win : GameResult.Loss;
        }

        await db.SaveChangesAsync();

        await _hubContext.Clients
             .Group(SignalRConstants.LobbyGroup(lobbyId))
             .SendAsync(SignalRConstants.TournamentUpdated);
    }

    public async Task<Dictionary<int, List<TournamentResult>>> GetTournamentHistoryAsync(int lobbyId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var rows = await db.TournamentResults
            .AsNoTracking()
            .Where(r => r.LobbyId == lobbyId && r.Result != null)
            .OrderBy(r => r.Game)
            .ThenBy(r => r.Id)
            .ToListAsync();

        return rows
            .GroupBy(r => r.Game)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public async Task<Guid?> GetSessionIdAsync(int lobbyId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        return await db.TournamentResults
            .AsNoTracking()
            .Where(r => r.LobbyId == lobbyId
            && (r.Result == null || r.Result == GameResult.Draw)
            && r.GameSessionId != Guid.Empty)
            .OrderByDescending(r => r.Game)
            .Select(r => (Guid?)r.GameSessionId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<TournamentResult>> GetWinnersOfLastRoundAsync(int lobbyId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var maxGame = await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId && r.Result != null)
            .Select(r => (int?)r.Game)
            .MaxAsync() ?? 0;

        if (maxGame == 0)
            return new List<TournamentResult>();

        return await db.TournamentResults
            .AsNoTracking()
            .Where(r => r.LobbyId == lobbyId && r.Game == maxGame && r.Result == GameResult.Win)
            .ToListAsync();
    }

    public async Task<List<TournamentResult>> GetDrawPlayersOfLastRoundAsync(int lobbyId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var maxGame = await db.TournamentResults
            .Where(r => r.LobbyId == lobbyId && r.Result != null)
            .Select(r => (int?)r.Game)
            .MaxAsync() ?? 0;

        if (maxGame == 0)
            return new List<TournamentResult>();

        return await db.TournamentResults
            .AsNoTracking()
            .Where(r => r.LobbyId == lobbyId && r.Game == maxGame && r.Result == GameResult.Draw)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetLatestScoresAsync(int lobbyId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        return await db.TournamentResults
            .AsNoTracking()
            .Where(r => r.LobbyId == lobbyId)
            .GroupBy(r => r.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                LatestScore = g.OrderByDescending(r => r.Id).First().Score
            })
            .ToDictionaryAsync(x => x.UserId, x => x.LatestScore);
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

    private static int HighestPowerOfTwoLE(int n)
    {
        if (n < 2) return 0;
        int p = 1;
        while ((p << 1) <= n && p < 128) p <<= 1;
        return p;
    }
}