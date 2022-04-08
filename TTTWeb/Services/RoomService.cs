using TTTService;
using System.Diagnostics.CodeAnalysis;

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

    public bool JoinRoom(string id, string guest, string guestName)
    {
        if (!rooms.TryGetValue(id, out var room)) return false;
        if (room.GameStarted || room.Guest is not null)
        {
            room.Audiences.Add(new(guest, guestName));
            return false;
        }
        room.Guest = guest;
        room.GuestName = guestName;
        return true;
    }

    public IEnumerable<RoomInfo> GetRooms() =>
        rooms.Select(r => new RoomInfo(r.Key, r.Value.OwnerName));

    public bool LeaveRoom(string id, string guest)
    {
        if (!rooms.TryGetValue(id, out var room)) return false;
        if (room.Guest != guest)
        {
            room.Audiences.RemoveAll(a => a.Player == guest);
            return false;
        }
        room.Guest = null;
        room.GuestName = null;
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
        if (!rooms.TryGetValue(id, out var room) || room.Owner != owner || room.Guest is null) return false;
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

    public bool GetBoard(string id, [NotNullWhen(true)] out IEnumerable<IEnumerable<GoBangTurnType>>? board)
    {
        if (!rooms.TryGetValue(id, out var room) || !room.GameStarted)
        {
            board = null;
            return false;
        }
        board = room.Game.Value.GetBoard();
        return true;
    }

    public GoBangTurnType? MakeMove(string id, string player, int x, int y, out GoBangTurnType turn)
    {
        if (!rooms.TryGetValue(id, out var room) || !room.GameStarted)
        {
            turn = GoBangTurnType.Null;
            return null;
        }
        var service = room.Game.Value;
        bool isCurrent;
        switch (service.NextTurnType)
        {
            case GoBangTurnType.Black:
                isCurrent = player == room.Owner;
                turn = GoBangTurnType.Black;
                break;
            case GoBangTurnType.White:
                isCurrent = player == room.Guest;
                turn = GoBangTurnType.White;
                break;
            default:
                isCurrent = false;
                turn = GoBangTurnType.Null;
                break;
        };
        if (!isCurrent) return null;
        return room.Game.Value.Judge(x, y);
    }
}