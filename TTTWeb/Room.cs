using TTTService;

namespace TTTWeb;

public class Room
{
    public string Owner { get; }
    public string OwnerName { get; }
    public string? Guest { get; set; }
    public string? GuestName { get; set; }
    public Lazy<GoBangService> Game { get; set; }
    public bool GameStarted { get; set; }
    public List<PlayerInfo> Audiences { get; }

    public Room(string owner, string ownerName)
    {
        Owner = owner;
        OwnerName = ownerName;
        Game = new(() => new(5, 5));
        Audiences = new();
    }
}

public record RoomInfo(string ID, string Name);
public record PlayerInfo(string Player, string PlayerName);

// public enum Player
// {
//     None,
//     Owner,
//     Guest
// }