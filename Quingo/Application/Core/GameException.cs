﻿namespace Quingo.Application.Core;

public class GameException : Exception
{
    public GameException(string? message) : base(message)
    {
        
    }

    public GameException(string? message, Exception? innerException) : base(message, innerException)
    {
        
    }
}
