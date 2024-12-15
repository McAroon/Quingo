using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Quingo.Infrastructure;
using Quingo.Infrastructure.Database;
using Quingo.Infrastructure.Database.Repos;
using Quingo.Shared.Entities;
using Quingo.Shared.Models;

namespace Quingo.Application.Core;

public class GameService : IDisposable
{
    private readonly ConcurrentDictionary<Guid, GameInstance> _state = new();
    public IReadOnlyList<GameInstance> Games => new List<GameInstance>(_state.Values).AsReadOnly();

    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    private readonly ILogger<GameService> _logger;

    private readonly GameLoop _loop;

    private readonly ICacheService _cache;

    private readonly UserConnectionTracker _userTracker;

    public GameService(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<GameService> logger,
        ICacheService cache, UserConnectionTracker userTracker)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _cache = cache;
        _loop = new GameLoop(logger, _state);
        _userTracker = userTracker;
    }

    public async Task<GameInstance> StartGame(int packId, PackPresetData preset, GameOptions options, string userId)
    {
        try
        {
            var startTime = Stopwatch.GetTimestamp();
            if (_state.Values.Any(x => x.IsStateActive && x.HostUserId == userId))
            {
                throw new GameException("User is already hosting a game");
            }

            var repo = new PackRepo(_dbContextFactory, _cache);

            await using var db = await repo.CreateDbContext();

            var pack = await repo.GetPack(packId);
            if (pack == null)
            {
                throw new GameException("Pack not found");
            }

            var getPackTime = Stopwatch.GetElapsedTime(startTime);
            var startTime2 = Stopwatch.GetTimestamp();

            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                throw new GameException("User not found");
            }

            var sessionId = Guid.NewGuid();

            var game = new GameInstance(sessionId, pack, preset, userId, user.UserName, _userTracker, options);
            if (!_state.TryAdd(sessionId, game))
            {
                throw new GameException("Error creating game");
            }

            var createGameTime = Stopwatch.GetElapsedTime(startTime2);
            var totalTime = Stopwatch.GetElapsedTime(startTime);

            _logger.LogInformation(
                "Created game id:{id} getPackTime:{getPackTime}s createGameTime:{createGameTime}s total:{totalTime}s",
                sessionId, getPackTime.TotalSeconds, createGameTime.TotalSeconds, totalTime.TotalSeconds);

            return game;
        }
        catch (Exception e) when (e is not GameException)
        {
            throw new GameException("Error creating game", e);
        }
    }

    public void PlayAgain(Guid gameSessionId)
    {
        try
        {
            var game = GetGameState(gameSessionId);
            if (game.IsStateActive)
            {
                throw new GameException("The current game is still active");
            }

            var sessionId = Guid.NewGuid();
            var newGame = new GameInstance(sessionId, game.Pack, game.Preset, game.HostUserId, game.HostName,
                _userTracker, game.Options);
            if (!_state.TryAdd(sessionId, newGame))
            {
                throw new GameException("Error creating game");
            }

            game.NotifyNewGameCreated(newGame);

            foreach (var player in game.Players)
            {
                var newPlayer = newGame.Join(player.PlayerUserId, player.PlayerName);

                player.NotifyNewGameCreated(newGame, newPlayer);
            }

            foreach (var spectator in game.Spectators)
            {
                newGame.Spectate(spectator);
            }

            _logger.LogInformation("Created repeat game id:{id} oldId:{oldId}", sessionId, game.GameSessionId);
        }
        catch (Exception e) when (e is not GameException)
        {
            throw new GameException("Error creating game", e);
        }
    }

    public PlayerInstance JoinGame(Guid gameSessionId, string userId, string userName, string? password = null)
    {
        ArgumentNullException.ThrowIfNull(userId, nameof(userId));
        ArgumentNullException.ThrowIfNull(userName, nameof(userName));

        try
        {
            var game = GetGameState(gameSessionId);
            var exPlayer = game.Players.FirstOrDefault(x => x.PlayerUserId == userId);
            if (exPlayer != null)
            {
                return exPlayer;
            }

            if (!game.CanJoin(userId))
            {
                throw new GameException("Unable to join, the room is full");
            }

            if (!string.IsNullOrWhiteSpace(game.Options.Password) && password != game.Options.Password)
            {
                throw new GameException("Incorrect password");
            }

            var player = game.Join(userId, userName);
            return player;
        }
        catch (Exception e) when (e is not GameException)
        {
            throw new GameException("Error joining the game", e);
        }
    }

    public void SpectateGame(Guid gameSessionId, string userId, string userName)
    {
        ArgumentNullException.ThrowIfNull(userId, nameof(userId));
        ArgumentNullException.ThrowIfNull(userName, nameof(userName));

        try
        {
            var game = GetGameState(gameSessionId);
            var player = game.Players.FirstOrDefault(x => x.PlayerUserId == userId);
            if (player != null)
            {
                throw new GameException("Unable to spectate, the user already joined as player");
            }

            var userInfo = new ApplicationUserInfo(userId, userName);
            game.Spectate(userInfo);
        }
        catch (Exception e) when (e is not GameException)
        {
            throw new GameException("Error trying to spectate the game", e);
        }
    }

    public async Task<GameInstance> GetGameState(Guid gameSessionId, string userId)
    {
        var game = GetGameState(gameSessionId);
        var hasAccess = await CheckUserAccess(game, userId);
        if (!hasAccess && game.Spectators.FirstOrDefault(x => x.UserId == userId) == null)
        {
            throw new GameException("User is not allowed to access the game");
        }

        return game;
    }

    private GameInstance GetGameState(Guid gameSessionId)
    {
        if (!_state.TryGetValue(gameSessionId, out var game))
        {
            throw new GameException("Game not found");
        }

        return game;
    }

    public PlayerInstance GetPlayerState(Guid gameSessionId, Guid playerSessionId, string userId)
    {
        var game = GetGameState(gameSessionId);
        var player = game.Players.FirstOrDefault(x => x.PlayerSessionId == playerSessionId && x.PlayerUserId == userId);
        if (player == null)
        {
            throw new GameException("Player not found");
        }

        return player;
    }

    public async Task EndGame(Guid gameSessionId, string userId)
    {
        try
        {
            var game = GetGameState(gameSessionId);

            var hasAccess = await CheckUserAccess(game, userId);
            if (!hasAccess)
            {
                throw new GameException("User is not allowed to end the game");
            }

            game.EndGame();
        }
        catch (Exception e) when (e is not GameException)
        {
            throw new GameException("Error while trying to end the game", e);
        }
    }

    private async Task<bool> CheckUserAccess(GameInstance game, string userId)
    {
        if (game.HostUserId == userId) return true;

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var userStore = new UserStore<ApplicationUser>(db);
        var user = await userStore.FindByIdAsync(userId);
        if (user == null) return false;

        var isAdmin = await userStore.IsInRoleAsync(user, "ADMIN");
        return isAdmin;
    }

    public void Dispose()
    {
        _loop.Dispose();
        foreach (var game in _state)
        {
            foreach (var player in game.Value.Players)
            {
                player.Dispose();
            }

            game.Value.Dispose();
        }
    }
}