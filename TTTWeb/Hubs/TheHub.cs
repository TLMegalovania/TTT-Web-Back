using Microsoft.AspNetCore.SignalR;
using TTTWeb.Services;
using TTTService;

namespace TTTWeb.Hubs;

public interface IClientHub
{
    Task ReceiveRooms(IEnumerable<RoomInfo> rooms);
    Task RoomCreated(RoomInfo room);
    Task JoinedRoom(RoomInfo room);
    Task LeftRoom(string id);
    Task RoomDeleted(string id);
    Task GameStarted(string id);
    Task GameEnded(string id);
    Task MadeMove(int x, int y, GoBangTurnType move, GoBangTurnType result);
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

    public Task Login(string userName)
    {
        Context.Items["username"] = userName;
        return Task.CompletedTask;
    }

    public IEnumerable<RoomInfo> GetRooms() => _roomService.GetRooms();

    public async Task<bool> CreateRoom()
    {
        if (!CheckAuth(out string ownerName)) return false;
        string id = _roomService.CreateRoom(Context.ConnectionId, ownerName);
        Context.Items["roomId"] = id;
        await Groups.AddToGroupAsync(Context.ConnectionId, id);
        await Clients.All.RoomCreated(new(id, ownerName));
        return true;
    }

    public async Task<bool> JoinRoom(string id)
    {
        if (!CheckAuth(out string guestName)) return false;
        if (!_roomService.JoinRoom(id, Context.ConnectionId, guestName)) return false;
        Context.Items["roomId"] = id;
        await Groups.AddToGroupAsync(Context.ConnectionId, id);
        await Clients.All.JoinedRoom(new(id, guestName));
        return true;
    }

    public async Task LeaveRoom()
    {
        if (!CheckRoom(out string id)) return;
        if (!_roomService.LeaveRoom(id, Context.ConnectionId)) return;
        await Clients.All.LeftRoom(new(id));
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, id);
    }

    public async Task DeleteRoom()
    {
        if (!CheckRoom(out string id)) return;
        if (!_roomService.DeleteRoom(id, Context.ConnectionId)) return;
        await Clients.All.RoomDeleted(new(id));
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, id);
    }

    public async Task StartGame()
    {
        if (!CheckRoom(out string id)) return;
        if (!_roomService.StartGame(id, Context.ConnectionId)) return;
        await Clients.All.GameStarted(id);
    }

    public async Task EndGame()
    {
        if (!CheckRoom(out string id)) return;
        if (!_roomService.EndGame(id, Context.ConnectionId)) return;
        await Clients.All.GameEnded(id);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (CheckRoom(out string id))
        {
            _roomService.EndGame(id, Context.ConnectionId);
            _roomService.LeaveRoom(id, Context.ConnectionId);
            _roomService.DeleteRoom(id, Context.ConnectionId);
        }
        return Task.CompletedTask;
    }

    public IEnumerable<IEnumerable<GoBangTurnType>>? GetBoard()
    {
        if (!CheckRoom(out string id)) return null;
        _roomService.GetBoard(id, out var board);
        return board;
    }

    public async Task<bool> MakeMove(int x, int y)
    {
        if (!CheckRoom(out string id)) return false;
        if (_roomService.MakeMove(id, Context.ConnectionId, x, y, out var turn) is not GoBangTurnType result) return false;
        await Clients.Group(id).MadeMove(x, y, turn, result);
        return true;
    }
}