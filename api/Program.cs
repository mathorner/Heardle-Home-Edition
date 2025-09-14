using Api.LibraryScan;
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
builder.Services.AddSingleton<ILibraryScanService, LibraryScanService>();

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

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Endpoints
app.MapSettingsEndpoints();

app.Run();

public partial class Program { }
