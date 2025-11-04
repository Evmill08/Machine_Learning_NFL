using backend.Services;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders().AddConsole().SetMinimumLevel(LogLevel.Debug);

// Add services to the container
builder.Services.AddMemoryCache();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Configure HttpClient with connection pooling and timeouts
builder.Services.AddHttpClient("DefaultClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(2),
    MaxConnectionsPerServer = 20, // Increase for parallel requests
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1)
});

// Register HttpClient services with optimized configuration
builder.Services.AddHttpClient<IOddsService, OddsService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(2));

builder.Services.AddHttpClient<IPredictorsService, PredictorsService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(2));

builder.Services.AddHttpClient<IScoreService, ScoreService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(2));

builder.Services.AddHttpClient<ITeamService, TeamService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(2));

// Register WeeksService
builder.Services.AddHttpClient<IWeeksService, WeeksService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(2));

// Register EventService with simplified DI
builder.Services.AddHttpClient<IEventService, EventService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(2));

// Register SeasonService with simplified DI
builder.Services.AddHttpClient<ISeasonService, SeasonService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(2));

// Register other services
builder.Services.AddScoped<IEndpointTestService, EndpointTestService>();
builder.Services.AddScoped<IPredictionDataService, PredictionDataService>();
builder.Services.AddScoped<IExcelService, ExcelService>();

var app = builder.Build();

// Configure middleware pipeline
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();