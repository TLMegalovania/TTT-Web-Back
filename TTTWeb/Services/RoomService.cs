using TTTService;
using StackExchange.Redis;

namespace TTTWeb.Services;

public class RoomService
{
    private IConnectionMultiplexer _redis;

    public RoomService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    private const string RoomPrefix = "room:";
    private const string AudiencesPrefix = "audiences:";

    public async Task<GoBangBoard?> GetGame(string id)
    {
        id = RoomPrefix + id;
        var db = _redis.GetDatabase();
        var game = await db.HashGetAsync(id, "Game");
        if (game.IsNull) return null;
        return GoBangBoard.Deserialize(game);
    }

    public async Task<string> CreateRoom(string owner, string ownerName)
    {
        Room room = new(owner, ownerName);
        string id = RoomPrefix + Guid.NewGuid().ToString();
        var rooms = _redis.GetDatabase();
        foreach (var (key, value) in room.StringProperties)
        {
            if (value is not null) await rooms.HashSetAsync(id, key, value);
        }
        return id;
    }


    public Task<bool> JoinRoom(string id, string guest, string guestName)
    {
        var rooms = _redis.GetDatabase();
        var tran = rooms.CreateTransaction();
        tran.AddCondition(Condition.KeyExists(id));
        tran.AddCondition(Condition.HashNotExists(id, "Guest"));
        _ = tran.HashSetAsync(id, "Guest", guest);
        _ = tran.HashSetAsync(id, "GuestName", guestName);
        return tran.ExecuteAsync();
    }

    public async IAsyncEnumerable<RoomInfo> GetRooms()
    {
        var endpoints = _redis.GetEndPoints();
        var rooms = _redis.GetDatabase();
        foreach (var endpoint in endpoints)
        {
            var server = _redis.GetServer(endpoint);
            var keys = server.KeysAsync(pattern: RoomPrefix + "*");
            await foreach (var key in keys)
            {
                var ownerName = await rooms.HashGetAsync(key, "OwnerName");
                var guestName = await rooms.HashGetAsync(key, "GuestName");
                var gameStarted = await rooms.HashExistsAsync(key, "Game");
                yield return new(key, ownerName, guestName.IsNull ? null : (string)guestName, gameStarted);
            }
        }
    }

    public Task<bool> LeaveRoom(string id, string guest)
    {
        var rooms = _redis.GetDatabase();
        var tran = rooms.CreateTransaction();
        tran.AddCondition(Condition.HashEqual(id, "Guest", guest));
        tran.HashDeleteAsync(id, "Guest");
        tran.HashDeleteAsync(id, "GuestName");
        return tran.ExecuteAsync();
    }

    public Task<bool> DeleteRoom(string id, string owner)
    {
        var rooms = _redis.GetDatabase();
        var tran = rooms.CreateTransaction();
        tran.AddCondition(Condition.HashEqual(id, "Owner", owner));
        tran.KeyDeleteAsync(id);
        return tran.ExecuteAsync();
    }

    public Task<bool> StartGame(string id, string owner, byte rows, byte columns)
    {
        var rooms = _redis.GetDatabase();
        var tran = rooms.CreateTransaction();
        tran.AddCondition(Condition.HashEqual(id, "Owner", owner));
        tran.AddCondition(Condition.HashExists(id, "Guest"));
        tran.AddCondition(Condition.HashNotExists(id, "Game"));
        tran.HashSetAsync(id, "Game", new GoBangBoard(rows, columns).Serialize());
        return tran.ExecuteAsync();
    }

    public async Task<bool> EndGame(string id, string player)
    {
        var rooms = _redis.GetDatabase();
        var tran1 = rooms.CreateTransaction();
        tran1.AddCondition(Condition.HashEqual(id, "Owner", player));
        tran1.AddCondition(Condition.HashExists(id, "Game"));
        _ = tran1.HashDeleteAsync(id, "Game");
        var tran2 = rooms.CreateTransaction();
        tran2.AddCondition(Condition.HashEqual(id, "Guest", player));
        tran2.AddCondition(Condition.HashExists(id, "Game"));
        _ = tran2.HashDeleteAsync(id, "Game");
        return await tran1.ExecuteAsync() || await tran2.ExecuteAsync();
    }

    public async IAsyncEnumerable<IEnumerable<GoBangTurnType>> GetBoard(string id)
    {
        var rooms = _redis.GetDatabase();
        var game = await rooms.HashGetAsync(id, "Game");
        if (game.IsNull)
        {
            yield break;
        }
        var board = GoBangBoard.Deserialize(game).GetBoard();
        foreach (var row in board)
        {
            yield return row;
        }
    }

    public async Task<MoveInfo?> MakeMove(string id, string player, bool isOwner, byte x, byte y)
    {
        var rooms = _redis.GetDatabase();
        var game = await rooms.HashGetAsync(id, "Game");
        if (game.IsNull)
        {
            return null;
        }
        string expectedPlayer;
        GoBangTurnType turn;
        if (isOwner)
        {
            expectedPlayer = await rooms.HashGetAsync(id, "Owner");
            turn = GoBangTurnType.Black;
        }
        else
        {
            expectedPlayer = await rooms.HashGetAsync(id, "Guest");
            turn = GoBangTurnType.White;
        }
        if (expectedPlayer != player)
        {
            return null;
        }
        var service = GoBangBoard.Deserialize(game);
        if (service.NextTurnType != turn)
        {
            return null;
        }
        if (service.Judge(x, y) is not GoBangTurnType result) return null;
        await rooms.HashSetAsync(id, "Game", service.Serialize());
        return new(x, y, turn, result);
    }
}