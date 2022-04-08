using TTTService;

namespace TTTWeb.Services;

public class RoomService
{
    private Dictionary<string, Room> rooms = new();

    public string CreateRoom(string owner, string ownerName)
    {
        Room room = new(owner, ownerName);
        string id = Guid.NewGuid().ToString();
        rooms.Add(id, room);
        return id;
    }

    public GoBangService? this[string id] =>
        rooms.TryGetValue(id, out var room) ? room.Game.Value : null;

    // public Player GetPlayer(string id, string player)
    // {
    //     if (!rooms.TryGetValue(id, out var room)) return Player.None;
    //     if (room.Owner == player) return Player.Owner;
    //     if (room.Guest == player) return Player.Guest;
    //     return Player.None;
    // }

    public bool JoinRoom(string id, string guest, string guestName)
    {
        if (!rooms.TryGetValue(id, out var room) || room.GameStarted) return false;
        lock (room)
        {
            if (room.IsFull) return false;
            room.Count++;
        }
        room.Guest = guest;
        room.GuestName = guestName;
        return true;
    }

    public IEnumerable<RoomInfo> GetRooms() =>
        rooms.Select(r => new RoomInfo(r.Key, r.Value.OwnerName));

    public bool LeaveRoom(string id, string guest)
    {
        if (!rooms.TryGetValue(id, out var room) || room.Guest != guest) return false;
        room.Guest = null;
        room.GuestName = null;
        room.Count--;
        return true;
    }

    public bool DeleteRoom(string id, string owner)
    {
        if (!rooms.TryGetValue(id, out var room) || room.Owner != owner) return false;
        rooms.Remove(id);
        return true;
    }

    public bool StartGame(string id, string owner)
    {
        if (!rooms.TryGetValue(id, out var room) || room.Owner != owner || !room.IsFull) return false;
        room.GameStarted = true;
        return true;
    }

    public bool EndGame(string id, string player)
    {
        if (!(rooms.TryGetValue(id, out var room) && (room.Owner == player || room.Guest == player))) return false;
        room.GameStarted = false;
        room.Game = new(() => new(5, 5));
        return true;
    }
}