using backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

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

builder.Services.AddHttpClient<IEventService, EventService>();
builder.Services.AddHttpClient<IOddsService, OddsService>();
builder.Services.AddHttpClient<IPredictorsService, PredictorsService>();
builder.Services.AddHttpClient<IScoreService, ScoreService>();
builder.Services.AddHttpClient<ISeasonService, SeasonService>();
builder.Services.AddHttpClient<ITeamService, TeamService>();
builder.Services.AddHttpClient<IWeeksService, WeeksService>();


app.MapControllers();

app.MapFallbackToFile("index.html");

app.UseCors("AllowFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();