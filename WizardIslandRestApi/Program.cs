using WizardIslandRestApi.Game;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(new GameManager());
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders(new[] { "location" });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
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
            context.Response.StatusCode = 200;
            return;
        }
    }
    context.Response.StatusCode = 400;
});

app.MapControllers();

app.Run();
