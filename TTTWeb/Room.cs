using TTTService;

namespace TTTWeb;

public class Room
{
    public string Owner { get; }
    public string OwnerName { get; }
    public string? Guest { get; set; }
    public string? GuestName { get; set; }

    public IEnumerable<(string, string?)> StringProperties
    {
        get
        {
            yield return (nameof(Owner), Owner);
            yield return (nameof(OwnerName), OwnerName);
            yield return (nameof(Guest), Guest);
            yield return (nameof(GuestName), GuestName);
        }
    }

    public Room(string owner, string ownerName)
    {
        Owner = owner;
        OwnerName = ownerName;
    }
}

public record RoomInfo(string ID, string OwnerName, string? GuestName, bool GameStarted);
public record MoveInfo(byte x, byte y, GoBangTurnType Turn, GoBangTurnType Result);
//public record PlayerInfo(string Player, string PlayerName);

// public enum Player
// {
//     None,
//     Owner,
//     Guest
// }