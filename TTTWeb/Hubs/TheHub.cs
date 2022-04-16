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
    private readonly ILogger<TheHub> _logger;
    public TheHub(RoomService roomService, ILogger<TheHub> logger)
    {
        _roomService = roomService;
        _logger = logger;
    }

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

    public async Task<string?> CreateRoom()
    {
        if (!CheckAuth(out string ownerName)) return null;
        if (CheckRoom(out _)) return null;
        string id = await _roomService.CreateRoom(Context.ConnectionId, ownerName);
        Context.Items["roomId"] = id;
        Context.Items["isOwner"] = true;
        await Groups.AddToGroupAsync(Context.ConnectionId, id);
        await Clients.All.RoomCreated(id, ownerName);
        _logger.LogInformation($"{ownerName} created room {id}");
        return id;
    }

    public async Task<bool> JoinRoom(string id)
    {
        if (!CheckAuth(out string guestName)) return false;
        if (CheckRoom(out _)) return false;
        _logger.LogInformation($"{guestName} joined room {id}");
        await Groups.AddToGroupAsync(Context.ConnectionId, id);
        if (await _roomService.JoinRoom(id, Context.ConnectionId, guestName))
        {
            Context.Items["roomId"] = id;
            Context.Items["isOwner"] = false;
            await Clients.All.JoinedRoom(id, guestName);
            return true;
        }
        return false;
    }

    public async Task LeaveRoom()
    {
        if (!CheckRoom(out string id)) return;
        await EndGame();
        if (await _roomService.LeaveRoom(id, Context.ConnectionId))
            await Clients.All.LeftRoom(new(id));
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, id);
        Context.Items.Remove("roomId");
        Context.Items.Remove("isOwner");
        _logger.LogInformation($"{Context.Items["username"]} left room {id}");
    }

    public async Task DeleteRoom()
    {
        if (!CheckRoom(out string id)) return;
        await EndGame();
        if (await _roomService.DeleteRoom(id, Context.ConnectionId))
            await Clients.All.RoomDeleted(new(id));
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, id);
        Context.Items.Remove("roomId");
        Context.Items.Remove("isOwner");
        _logger.LogInformation($"{Context.Items["username"]} deleted room {id}");
    }

    public async Task StartGame(int rows, int cols)
    {
        if (!CheckRoom(out string id)) return;
        _logger.LogInformation($"{Context.Items["username"]} trying to start game in room {id}");
        if (!await _roomService.StartGame(id, Context.ConnectionId, (byte)rows, (byte)cols)) return;
        await Clients.All.GameStarted(id);
    }

    public async Task EndGame()
    {
        if (!CheckRoom(out string id)) return;
        _logger.LogInformation($"{Context.Items["username"]} trying to end game in room {id}");
        if (!await _roomService.EndGame(id, Context.ConnectionId)) return;
        await Clients.All.GameEnded(id);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"{Context.Items["username"]} disconnected");
        if (!CheckRoom(out string id)) return;
        await _roomService.EndGame(id, Context.ConnectionId);
        await _roomService.LeaveRoom(id, Context.ConnectionId);
        await _roomService.DeleteRoom(id, Context.ConnectionId);
    }

    public async IAsyncEnumerable<IEnumerable<GoBangTurnType>> GetBoard()
    {
        if (!CheckRoom(out string id)) yield break;
        _logger.LogInformation($"{Context.Items["username"]} requested board in room {id}");
        await foreach (var row in _roomService.GetBoard(id)) yield return row;
    }

    public async Task<bool> MakeMove(int x, int y)
    {
        if (!CheckRoom(out string id)) return false;
        _logger.LogInformation($"{Context.Items["username"]} trying to make move ({x}, {y}) in room {id}");
        if (!CheckOwner(out bool isOwner)) return false;
        if (await _roomService.MakeMove(id, Context.ConnectionId, isOwner, (byte)x, (byte)y) is not MoveInfo move) return false;
        await Clients.Group(id).MadeMove(move);
        if (move.Result != GoBangTurnType.Null)
            await EndGame();
        _logger.LogInformation($"{Context.Items["username"]} made move ({x}, {y}) in room {id}");
        return true;
    }
}