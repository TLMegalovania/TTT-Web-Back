using TTTWeb.Services;
using TTTWeb.Hubs;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
IConfiguration config = builder.Configuration;
services.AddSignalR().AddStackExchangeRedis(config.GetConnectionString("Redis"), (options) =>
{
    options.Configuration.ChannelPrefix = "tttweb-connection:";
});
services
    .AddSingleton<RoomService>()
    .AddSingleton<IConnectionMultiplexer, ConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(provider.GetRequiredService<IConfiguration>().GetConnectionString("Redis"))
    );

var app = builder.Build();

app.MapHub<TheHub>("/api/hub");

app.Run();
