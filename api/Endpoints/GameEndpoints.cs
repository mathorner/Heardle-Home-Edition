using Api.Game;
using Api.LibraryIndex;
using Api.LibraryScan;

namespace Api.Endpoints;

public static class GameEndpoints
{
    public static IEndpointRouteBuilder MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/game");

        group.MapPost("/start", async (
            ILibraryIndexProvider indexProvider,
            IRandomTrackSelector selector,
            IGameSessionStore sessionStore,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var snapshot = await indexProvider.GetAsync(ct);
            if (snapshot.Status == LibraryIndexStatus.NotReady)
            {
                return Results.Json(
                    new ErrorResponse(snapshot.ErrorCode ?? LibraryIndexErrorCodes.LibraryNotReady,
                        snapshot.ErrorMessage ?? "Run a scan before playing."),
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            if (snapshot.Status == LibraryIndexStatus.Empty || snapshot.Tracks.Count == 0)
            {
                return Results.Json(
                    new ErrorResponse(
                        snapshot.ErrorCode ?? LibraryIndexErrorCodes.NoTracksIndexed,
                        snapshot.ErrorMessage ?? "No tracks available. Scan your library."),
                    statusCode: StatusCodes.Status409Conflict);
            }

            var track = selector.TrySelect(snapshot.Tracks);
            if (track is null)
            {
                return Results.Json(
                    new ErrorResponse(
                        LibraryIndexErrorCodes.NoTracksIndexed,
                        "No tracks available. Scan your library."),
                    statusCode: StatusCodes.Status409Conflict);
            }

            var session = sessionStore.Create(track);
            var response = GameSessionView.FromSession(session);
            var location = $"/game/{session.Id}";

            var logger = loggerFactory.CreateLogger("GameEndpoints");
            logger.LogInformation("Game session {SessionId} started for track {TrackId}", session.Id, track.Id);

            return Results.Created(location, response);
        });

        group.MapGet("/{id}", (string id, IGameSessionStore sessionStore) =>
        {
            if (!Guid.TryParse(id, out var gameId))
            {
                return Results.Json(new ErrorResponse("InvalidGameId", "Game id must be a GUID."), statusCode: StatusCodes.Status400BadRequest);
            }

            if (!sessionStore.TryGet(gameId, out var session) || session is null)
            {
                return Results.Json(new ErrorResponse("GameNotFound", "Game session not found."), statusCode: StatusCodes.Status404NotFound);
            }

            return Results.Ok(GameSessionView.FromSession(session));
        });

        return app;
    }

    private sealed record ErrorResponse(string Code, string Message);

    private sealed record GameSessionView(Guid GameId, string Status, int Attempt, int MaxAttempts, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
    {
        public static GameSessionView FromSession(GameSession session) =>
            new(session.Id, session.Status, session.Attempt, session.MaxAttempts, session.CreatedAt, session.UpdatedAt);
    }
}
