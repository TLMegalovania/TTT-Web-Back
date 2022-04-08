namespace TTTService;
public enum GoBangTurnType
{
    Null,
    Black,
    White,
    Tie
}

public static class GoBangTurnTypeExtensions
{
    public static GoBangTurnType Opposite(this GoBangTurnType t) => t switch
    {
        GoBangTurnType.Black => GoBangTurnType.White,
        GoBangTurnType.White => GoBangTurnType.Black,
        _ => throw new ArgumentOutOfRangeException(nameof(t)),
    };
}
