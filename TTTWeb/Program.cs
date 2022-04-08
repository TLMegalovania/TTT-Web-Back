using TTTWeb.Services;
using TTTWeb.Hubs;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
services.AddSignalR();
services
    .AddSingleton<RoomService>();

var app = builder.Build();

app.MapHub<TheHub>("/api");
//app.MapHub<GameHub>("/api/game");

app.Run();
