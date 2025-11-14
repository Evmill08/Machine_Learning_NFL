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

// Configure a default HttpClient configuration for reuse (optional)
builder.Services.AddHttpClient("DefaultClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(2),
    MaxConnectionsPerServer = 20,
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1)
});

// Register services that require HttpClient using AddHttpClient<TInterface, TImplementation>()
// This ensures HttpClient is injected into their constructors.

builder.Services.AddHttpClient<IPredictorsService, PredictorsService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(2));

builder.Services.AddHttpClient<IScoreService, ScoreService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(2));

builder.Services.AddHttpClient<ITeamService, TeamService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(2));

builder.Services.AddHttpClient<IWeeksService, WeeksService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(2));

builder.Services.AddHttpClient<IEventService, EventService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(2));

builder.Services.AddHttpClient<ISeasonService, SeasonService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(2));

// OddsService needs HttpClient + IEventService - register as HttpClient factory as well
builder.Services.AddHttpClient<IOddsService, OddsService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(2));

// PredictionService needs HttpClient + several other services
builder.Services.AddHttpClient<IPredictionService, PredictionService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(2));

builder.Services.AddHttpClient<IWeatherService, WeatherService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(2));

// Register the remaining services that do NOT need HttpClient (regular DI)
builder.Services.AddScoped<IEndpointTestService, EndpointTestService>();
builder.Services.AddScoped<IPredictionDataService, PredictionDataService>();
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddScoped<IGameDataService, GameDataService>();

builder.Services.AddScoped<Lazy<IEventService>>(provider => 
    new Lazy<IEventService>(() => provider.GetRequiredService<IEventService>()));

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

var endpoints = app.Services.GetRequiredService<Microsoft.AspNetCore.Routing.EndpointDataSource>();
foreach (var endpoint in endpoints.Endpoints)
{
    if (endpoint is Microsoft.AspNetCore.Routing.RouteEndpoint routeEndpoint)
    {
        Console.WriteLine($"Route: {routeEndpoint.RoutePattern.RawText}");
    }
}

app.Run();