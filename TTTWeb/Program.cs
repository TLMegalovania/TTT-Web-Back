using TTTWeb.Services;
using TTTWeb.Hubs;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
services.AddSignalR();
services
    .AddSingleton<RoomService>()
    .AddSingleton<IConnectionMultiplexer, ConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect("redis"));

var app = builder.Build();

app.MapHub<TheHub>("/api");

app.Run();
