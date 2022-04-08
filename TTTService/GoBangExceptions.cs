namespace TTTService;

public class NeverException : Exception
{
    public NeverException(string? message = default) : base($"Reached ideally unreachable code : {message}") { }
}
public class CheatException : Exception
{
    public CheatException() : base("There's an invalid move.") { }
}
