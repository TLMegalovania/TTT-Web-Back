using TTTWeb.Services;
using TTTWeb.Hubs;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
services.AddSignalR();
services
    .AddSingleton<RoomService>()
    .AddSingleton<IConnectionMultiplexer, ConnectionMultiplexer>(provider =>
    {
        return ConnectionMultiplexer.Connect(provider.GetRequiredService<IConfiguration>().GetConnectionString("Redis"));
    });

var app = builder.Build();

app.MapHub<TheHub>("/api/hub");

app.Run();
