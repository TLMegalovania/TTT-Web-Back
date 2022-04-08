using TTTService;

namespace TTTWeb;

public class Room
{
    public string Owner { get; }
    public string OwnerName { get; }
    public string? Guest { get; set; }
    public string? GuestName { get; set; }
    public Lazy<GoBangService> Game { get; set; }
    public int Count { get; set; }
    public bool IsFull => Count >= 2;
    public bool GameStarted { get; set; }

    public Room(string owner, string ownerName)
    {
        Owner = owner;
        OwnerName = ownerName;
        Game = new(() => new(5, 5));
        Count = 1;
    }
}

public record RoomInfo(string ID, string Name);

// public enum Player
// {
//     None,
//     Owner,
//     Guest
// }