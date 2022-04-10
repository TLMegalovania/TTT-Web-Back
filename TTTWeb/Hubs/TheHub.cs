using Microsoft.AspNetCore.SignalR;
using TTTWeb.Services;
using TTTService;

namespace TTTWeb.Hubs;

public interface IClientHub
{
    Task RoomCreated(string id, string ownerName);
    Task JoinedRoom(string id, string guestName);
    Task LeftRoom(string id);
    Task RoomDeleted(string id);
    Task GameStarted(string id);
    Task GameEnded(string id);
    Task MadeMove(MoveInfo move);
}

public class TheHub : Hub<IClientHub>
{
    private readonly RoomService _roomService;
    public TheHub(RoomService roomService) => _roomService = roomService;

    private bool CheckAuth(out string username)
    {
        if (!Context.Items.TryGetValue("username", out var obj) || obj is not string name)
        {
            username = "";
            return false;
        }
        username = name;
        return true;
    }

    private bool CheckRoom(out string id)
    {
        if (!Context.Items.TryGetValue("roomId", out var obj) || obj is not string roomId)
        {
            id = "";
            return false;
        }
        id = roomId;
        return true;
    }

    private bool CheckOwner(out bool isOwner)
    {
        if (!Context.Items.TryGetValue("isOwner", out var obj) || obj is not bool owner)
        {
            isOwner = false;
            return false;
        }
        isOwner = owner;
        return true;
    }

    public Task Login(string userName)
    {
        Context.Items["username"] = userName;
        return Task.CompletedTask;
    }

    public IAsyncEnumerable<RoomInfo> GetRooms() => _roomService.GetRooms();

    public async Task<bool> CreateRoom()
    {
        if (!CheckAuth(out string ownerName)) return false;
        if (CheckRoom(out _)) return false;
        string id = await _roomService.CreateRoom(Context.ConnectionId, ownerName);
        Context.Items["roomId"] = id;
        Context.Items["isOwner"] = true;
        await Groups.AddToGroupAsync(Context.ConnectionId, id);
        await Clients.All.RoomCreated(id, ownerName);
        return true;
    }

    public async Task<bool> JoinRoom(string id)
    {
        if (!CheckAuth(out string guestName)) return false;
        if (CheckRoom(out _)) return false;
        if (await _roomService.JoinRoom(id, Context.ConnectionId, guestName))
        {
            Context.Items["roomId"] = id;
            Context.Items["isOwner"] = false;
            await Clients.All.JoinedRoom(id, guestName);
            return true;
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, id);
        return false;
    }

    public async Task LeaveRoom()
    {
        if (!CheckRoom(out string id)) return;
        if (await _roomService.LeaveRoom(id, Context.ConnectionId))
            await Clients.All.LeftRoom(new(id));
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, id);
    }

    public async Task DeleteRoom()
    {
        if (!CheckRoom(out string id)) return;
        if (await _roomService.DeleteRoom(id, Context.ConnectionId))
            await Clients.All.RoomDeleted(new(id));
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, id);
    }

    public async Task StartGame(byte rows, byte cols)
    {
        if (!CheckRoom(out string id)) return;
        if (!await _roomService.StartGame(id, Context.ConnectionId, rows, cols)) return;
        await Clients.All.GameStarted(id);
    }

    public async Task EndGame()
    {
        if (!CheckRoom(out string id)) return;
        if (!await _roomService.EndGame(id, Context.ConnectionId)) return;
        await Clients.All.GameEnded(id);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (!CheckRoom(out string id)) return;
        await _roomService.EndGame(id, Context.ConnectionId);
        await _roomService.LeaveRoom(id, Context.ConnectionId);
        await _roomService.DeleteRoom(id, Context.ConnectionId);
    }

    public async IAsyncEnumerable<IEnumerable<GoBangTurnType>> GetBoard()
    {
        if (!CheckRoom(out string id)) yield break;
        await foreach (var row in _roomService.GetBoard(id)) yield return row;
    }

    public async Task<bool> MakeMove(byte x, byte y)
    {
        if (!CheckRoom(out string id)) return false;
        if (!CheckOwner(out bool isOwner)) return false;
        if (await _roomService.MakeMove(id, Context.ConnectionId, isOwner, x, y) is not MoveInfo move) return false;
        await Clients.Group(id).MadeMove(move);
        if (move.Result != GoBangTurnType.Null)
            await Clients.All.GameEnded(id);
        return true;
    }
}