public static class SettingsEndpoints
{
    public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/settings/library-path", async (ISettingsService service, CancellationToken ct) =>
        {
            var path = await service.LoadAsync(ct);
            return path is null
                ? Results.NotFound()
                : Results.Ok(new { path });
        })
        .WithName("GetLibraryPath");

        app.MapPost("/settings/library-path", async (
            LibraryPathRequest req,
            ISettingsService service,
            ILogger<Program> logger,
            CancellationToken ct) =>
        {
            var (ok, code, message, normalized) = service.ValidatePath(req.Path);
            if (!ok)
            {
                return Results.BadRequest(new ErrorResponse(false, code!, message!));
            }

            try
            {
                await service.SaveAsync(normalized!, ct);
                return Results.Ok(new { saved = true, path = normalized });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save settings");
                return Results.BadRequest(new ErrorResponse(false, "Unknown", "Failed to save settings."));
            }
        })
        .WithName("SaveLibraryPath");

        return app;
    }
}

public record LibraryPathRequest(string Path);
public record ErrorResponse(bool Saved, string Code, string Message);
