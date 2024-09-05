using Microsoft.EntityFrameworkCore;
using Quingo.Data;
using System.Collections.Concurrent;

namespace Quingo.Application.State;

public class GameStateService
{
    private static readonly ConcurrentDictionary<Guid, GameState> _state = new();

    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public GameStateService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<GameState> StartGame(int packId, string userId)
    {
        try
        {
            if (_state.Values.Any(x => x.HostUserId == userId))
            {
                throw new GameStateException("User is already hosting a game");
            }

            using var db = await _dbContextFactory.CreateDbContextAsync();

            var pack = await db.PacksWithIncludes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == packId);
            if (pack == null)
            {
                throw new GameStateException("Pack not found");
            }

            var preset = pack.Presets.FirstOrDefault();
            if (preset == null)
            {
                throw new GameStateException("Preset not found");
            }

            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                throw new GameStateException("User not found");
            }

            var sessionId = Guid.NewGuid();

            var game = new GameState(sessionId, pack, preset, userId);
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

    public void JoinGame(Guid gameSessionId, string userId)
    {
        try
        {
            var game = GetGameState(gameSessionId);
            if (game.Players.Any(x => x.PlayerUserId == userId))
            {
                throw new GameStateException("Player has already joined the game");
            }

            var playerSessionId = Guid.NewGuid();
            var player = new PlayerState(playerSessionId, game, userId);
            game.Join(player);
        }
        catch (Exception e) when (e is not GameStateException)
        {

            throw new GameStateException("Error joining the game", e);
        }
    }

    public GameState GetGameState(Guid gameSessionId)
    {
        if (!_state.TryGetValue(gameSessionId, out var game))
        {
            throw new GameStateException("Game state not found");
        }
        return game;
    }

    public PlayerState GetPlayerState(Guid gameSessionId, Guid playerSessionId, string userId)
    {
        var game = GetGameState(gameSessionId);
        var player = game.Players.FirstOrDefault(x => x.PlayerSessionId == playerSessionId && x.PlayerUserId == userId);
        if (player == null)
        {
            throw new GameStateException("Player state not found");
        }
        return player;
    }

    public void EndGame(Guid gameSessionId, string userId)
    {
        try
        {
            var game = GetGameState(gameSessionId);
            if (game.HostUserId != userId)
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

    public void RemoveGame(GameState game)
    {
        if (game.State != GameStateEnum.Finished)
        {
            throw new GameStateException("Only finished games can be removed");
        }

        Task.Run(async () => 
        {
            await Task.Delay(1000 * 10);
            game.Dispose();
            _state.TryRemove(game.GameSessionId, out _);
        });
    }
}
