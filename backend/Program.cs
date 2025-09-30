using backend.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Add CORS before building the app
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

// Register all HttpClient services before building the app
builder.Services.AddHttpClient<IOddsService, OddsService>();
builder.Services.AddHttpClient<IPredictorsService, PredictorsService>();
builder.Services.AddHttpClient<IScoreService, ScoreService>();
builder.Services.AddHttpClient<ITeamService, TeamService>();

// Register WeeksService first (no dependencies on Event/Season services)
builder.Services.AddHttpClient<WeeksService>();
builder.Services.AddScoped<IWeeksService, WeeksService>();

// Register EventService with its dependencies
builder.Services.AddHttpClient<EventService>()
    .AddTypedClient<IEventService>((client, sp) =>
    {
        var teamService = sp.GetRequiredService<ITeamService>();
        var scoreService = sp.GetRequiredService<IScoreService>();
        var oddsService = sp.GetRequiredService<IOddsService>();
        var predictorsService = sp.GetRequiredService<IPredictorsService>();

        return new EventService(client, teamService, scoreService, oddsService, predictorsService);
    });

// Register SeasonService with its dependencies
builder.Services.AddHttpClient<SeasonService>()
    .AddTypedClient<ISeasonService>((client, sp) =>
    {
        var weeksService = sp.GetRequiredService<IWeeksService>();
        return new SeasonService(client, weeksService);
    });

builder.Services.AddScoped<IEndpointTestService, EndpointTestService>();

// Build the app AFTER all service registrations
var app = builder.Build();

// Configure middleware
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();