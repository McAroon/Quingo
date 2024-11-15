using Microsoft.EntityFrameworkCore;
using Quingo.Infrastructure.Database;
using Quingo.Shared.Entities;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Quingo.Infrastructure.Database.Repos;

namespace Quingo.Application.State;

public class GameStateService
{
    private static readonly ConcurrentDictionary<Guid, GameState> _state = new();
    public IReadOnlyList<GameState> Games => new List<GameState>(_state.Values).AsReadOnly();

    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    private readonly ILogger<GameStateService> _logger;

    public GameStateService(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<GameStateService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task<GameState> StartGame(int packId, PackPresetData preset, string userId)
    {
        try
        {
            if (_state.Values.Any(x => (x.State is not GameStateEnum.Finished and not GameStateEnum.Canceled) && x.HostUserId == userId))
            {
                throw new GameStateException("User is already hosting a game");
            }
            
            var repo = new PackRepo(_dbContextFactory);
            
            await using var db = await repo.CreateDbContext();

            var pack = await repo.GetPack(packId);
            if (pack == null)
            {
                throw new GameStateException("Pack not found");
            }

            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                throw new GameStateException("User not found");
            }

            var sessionId = Guid.NewGuid();

            var game = new GameState(_logger, sessionId, pack, preset, userId, user.UserName);
            if (!_state.TryAdd(sessionId, game))
            {
                throw new GameStateException("Error creating game");
            }
            return game;
        }
        catch (Exception e) when (e is not GameStateException)
        {

            throw new GameStateException("Error creating game", e);
        }
    }

    public PlayerState JoinGame(Guid gameSessionId, string userId, string userName)
    {
        try
        {
            var game = GetGameState(gameSessionId);
            var exPlayer = game.Players.FirstOrDefault(x => x.PlayerUserId == userId);
            if (exPlayer != null)
            {
                return exPlayer;
            }

            if (!game.CanJoin)
            {
                throw new GameStateException("Unable to join, the room is full");
            }

            var playerSessionId = Guid.NewGuid();
            var player = new PlayerState(playerSessionId, game, userId, userName);
            game.Join(player);
            return player;
        }
        catch (Exception e) when (e is not GameStateException)
        {

            throw new GameStateException("Error joining the game", e);
        }
    }

    public GameState GetGameState(Guid gameSessionId, string userId)
    {
        var game = GetGameState(gameSessionId);
        if (game.HostUserId != userId)
        {
            throw new GameStateException("User is not the host");
        }
        return game;
    }

    private GameState GetGameState(Guid gameSessionId)
    {
        if (!_state.TryGetValue(gameSessionId, out var game))
        {
            throw new GameStateException("Game not found");
        }
        return game;
    }

    public PlayerState GetPlayerState(Guid gameSessionId, Guid playerSessionId, string userId)
    {
        var game = GetGameState(gameSessionId);
        var player = game.Players.FirstOrDefault(x => x.PlayerSessionId == playerSessionId && x.PlayerUserId == userId);
        if (player == null)
        {
            throw new GameStateException("Player not found");
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
                throw new GameStateException("User is not allowed to end the game");
            }

            game.EndGame();
            RemoveGame(game);
        }
        catch (Exception e) when (e is not GameStateException)
        {

            throw new GameStateException("Error while trying to end the game", e);
        }
    }

    private async Task<bool> CheckUserAccess(GameState game, string userId)
    {
        if (game.HostUserId == userId) return true;
        
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var userStore = new UserStore<ApplicationUser>(db);
        var user = await userStore.FindByIdAsync(userId);
        if (user == null) return false;
        
        var isAdmin = await userStore.IsInRoleAsync(user, "admin");
        return isAdmin;
    }

    public void RemoveGame(GameState game)
    {
        if (game.State is not GameStateEnum.Finished and not GameStateEnum.Canceled)
        {
            return;
        }

        _ = Task.Run(async () => 
        {
            await Task.Delay(1000 * 10);
            game.Dispose();
            _state.TryRemove(game.GameSessionId, out _);
        });
    }
}
