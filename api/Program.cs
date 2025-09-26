using Api.Endpoints;
using Api.Game;
using Api.LibraryIndex;
using Api.LibraryScan;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);

// Logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// CORS configuration (Development only at runtime below)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Services
builder.Services.AddSingleton<ISettingsService, SettingsService>();
builder.Services.AddSingleton<ITrackMetadataExtractor, TagLibMetadataExtractor>();
builder.Services.AddSingleton<IIndexWriter, JsonIndexWriter>();
builder.Services.AddSingleton<ILibraryIndexProvider, LibraryIndexProvider>();
builder.Services.AddSingleton<ILibraryScanService, LibraryScanService>();
builder.Services.AddSingleton<IScanManager, ScanManager>();
builder.Services.AddSingleton<IGameSessionStore, InMemoryGameSessionStore>();
builder.Services.AddSingleton<IRandomTrackSelector, RandomTrackSelector>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Heardle Home Edition API",
        Version = "v1",
        Description = "Backend endpoints powering the Heardle Home Edition experience."
    });
});

var app = builder.Build();

// Enable CORS for local dev; use HSTS+HTTPS redirection elsewhere
if (app.Environment.IsDevelopment())
{
    app.UseCors();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Heardle Home Edition API v1");
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Endpoints
app.MapSettingsEndpoints();
app.MapLibraryEndpoints();
app.MapGameEndpoints();

app.Run();

public partial class Program { }
