using Api.LibraryScan;

namespace Api.Endpoints;

public static class LibraryEndpoints
{
    public static IEndpointRouteBuilder MapLibraryEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /library/status – return current scan status snapshot
        app.MapGet("/library/status", (IScanManager manager) =>
        {
            var s = manager.GetStatus();
            return Results.Ok(s);
        })
        .WithName("GetLibraryScanStatus");

        // POST /library/scan – start a background scan if not running
        app.MapPost("/library/scan", async (
            IScanManager manager,
            ILibraryScanService scanService,
            ISettingsService settings,
            CancellationToken ct) =>
        {
            // Ensure library path is configured before allowing a run
            var root = await settings.LoadAsync(ct);
            if (string.IsNullOrWhiteSpace(root))
            {
                return Results.BadRequest(new { code = "MissingLibraryPath", message = "Library path is not configured" });
            }

            var started = manager.TryStart(token => scanService.ScanAsync(token));
            if (!started)
            {
                return Results.Conflict(new { status = "running" });
            }

            var status = manager.GetStatus();
            return Results.Accepted(value: new { status = status.Status, startedAt = status.StartedAt });
        })
        .WithName("StartLibraryScan");

        return app;
    }
}

