namespace Quingo.Application.State;

public class GameStateException : Exception
{
    public GameStateException(string? message) : base(message)
    {
        
    }

    public GameStateException(string? message, Exception? innerException) : base(message, innerException)
    {
        
    }
}
