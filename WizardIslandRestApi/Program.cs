using WizardIslandRestApi.Game;
using WizardIslandRestApi.Game.Augments;

AugmentSystem.LoadAugments();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton(new GameManager());
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders(new[] { "location" });
    });
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.UseAuthorization();

app.UseWebSockets();
app.Map("/joinGame", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        string gameId = context.Request.Query["id"].FirstOrDefault() ?? "-1";
        // join game
        if (int.TryParse(gameId, out var id))
        {
            using var socket = await context.WebSockets.AcceptWebSocketAsync();
            await GameManager.Instance.JoinAndPlayGame(id, socket);
            return;
        }
    }
    context.Response.StatusCode = 400;
});

app.MapControllers();

app.Run();
