using System.Net;
using System.Net.Http.Json;
using Api.Endpoints;
using Api.Game;
using Api.LibraryIndex;
using Api.LibraryScan;
using Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

public class GameEndpointsTests
{
    private sealed class StubIndexProvider : ILibraryIndexProvider
    {
        public LibraryIndexSnapshot Snapshot { get; set; } = new(
            LibraryIndexStatus.NotReady,
            Array.Empty<TrackRecord>(),
            0,
            null,
            LibraryIndexErrorCodes.LibraryNotReady,
            "Library index not ready");

        public Task<LibraryIndexSnapshot> GetAsync(CancellationToken ct = default) => Task.FromResult(Snapshot);

        public void Invalidate()
        {
            // no-op for tests
        }
    }

    private sealed class StubTrackSelector : IRandomTrackSelector
    {
        public TrackRecord? NextTrack { get; set; }

        public TrackRecord? TrySelect(IReadOnlyList<TrackRecord> tracks)
        {
            return NextTrack ?? tracks.FirstOrDefault();
        }
    }

    private sealed class GameFactory : TestWebApplicationFactory
    {
        private readonly StubIndexProvider _provider;
        private readonly StubTrackSelector _selector;

        public GameFactory(StubIndexProvider provider, StubTrackSelector selector)
        {
            _provider = provider;
            _selector = selector;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<ILibraryIndexProvider>(_ => _provider);
                services.AddSingleton<IRandomTrackSelector>(_ => _selector);
            });
        }
    }

    private static TrackRecord Track(string id = "track-1") =>
        new(id, "Song", "Artist", $"/music/{id}.mp3");

    [Fact]
    public async Task StartGame_Returns201AndCreatesSession()
    {
        var provider = new StubIndexProvider
        {
            Snapshot = new LibraryIndexSnapshot(
                LibraryIndexStatus.Ready,
                new[] { Track() },
                1,
                DateTimeOffset.UtcNow,
                null,
                null)
        };
        var selector = new StubTrackSelector { NextTrack = Track() };

        await using var factory = new GameFactory(provider, selector);
        var client = factory.CreateClient();

        var response = await client.PostAsync("/game/start", content: null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var payload = await response.Content.ReadFromJsonAsync<GameSessionResponse>();
        Assert.NotNull(payload);
        Assert.Equal("active", payload!.Status);
        Assert.Equal(1, payload.Attempt);
        Assert.Equal(6, payload.MaxAttempts);

        var sessionResponse = await client.GetFromJsonAsync<GameSessionResponse>($"/game/{payload!.GameId}");
        Assert.NotNull(sessionResponse);
        Assert.Equal(payload.GameId, sessionResponse!.GameId);
    }

    [Fact]
    public async Task StartGame_Returns503_WhenLibraryNotReady()
    {
        var provider = new StubIndexProvider
        {
            Snapshot = new LibraryIndexSnapshot(
                LibraryIndexStatus.NotReady,
                Array.Empty<TrackRecord>(),
                0,
                null,
                LibraryIndexErrorCodes.LibraryNotReady,
                "Run a scan"
            )
        };
        var selector = new StubTrackSelector();

        await using var factory = new GameFactory(provider, selector);
        var client = factory.CreateClient();

        var response = await client.PostAsync("/game/start", content: null);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ErrorEnvelope>();
        Assert.Equal("LibraryNotReady", payload!.Code);
    }

    [Fact]
    public async Task StartGame_Returns409_WhenNoTracksAvailable()
    {
        var provider = new StubIndexProvider
        {
            Snapshot = new LibraryIndexSnapshot(
                LibraryIndexStatus.Empty,
                Array.Empty<TrackRecord>(),
                0,
                DateTimeOffset.UtcNow,
                LibraryIndexErrorCodes.NoTracksIndexed,
                "No tracks"
            )
        };
        var selector = new StubTrackSelector();

        await using var factory = new GameFactory(provider, selector);
        var client = factory.CreateClient();

        var response = await client.PostAsync("/game/start", content: null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ErrorEnvelope>();
        Assert.Equal("NoTracksIndexed", payload!.Code);
    }

    [Fact]
    public async Task GetGame_ReturnsSnapshot_WhenSessionExists()
    {
        var provider = new StubIndexProvider
        {
            Snapshot = new LibraryIndexSnapshot(
                LibraryIndexStatus.Ready,
                new[] { Track() },
                1,
                DateTimeOffset.UtcNow,
                null,
                null)
        };
        var selector = new StubTrackSelector { NextTrack = Track() };

        await using var factory = new GameFactory(provider, selector);
        var client = factory.CreateClient();

        var start = await client.PostAsync("/game/start", content: null);
        var started = await start.Content.ReadFromJsonAsync<GameSessionResponse>();
        Assert.NotNull(started);

        var response = await client.GetAsync($"/game/{started!.GameId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<GameSessionResponse>();
        Assert.NotNull(payload);
        Assert.Equal(started.GameId, payload!.GameId);
    }

    [Fact]
    public async Task GetGame_Returns400_ForInvalidId()
    {
        var provider = new StubIndexProvider();
        var selector = new StubTrackSelector();

        await using var factory = new GameFactory(provider, selector);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/game/not-a-guid");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ErrorEnvelope>();
        Assert.Equal("InvalidGameId", payload!.Code);
    }

    [Fact]
    public async Task GetGame_Returns404_WhenSessionMissing()
    {
        var provider = new StubIndexProvider();
        var selector = new StubTrackSelector();

        await using var factory = new GameFactory(provider, selector);
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/game/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ErrorEnvelope>();
        Assert.Equal("GameNotFound", payload!.Code);
    }

    private sealed record GameSessionResponse(Guid GameId, string Status, int Attempt, int MaxAttempts, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

    private sealed record ErrorEnvelope(string Code, string Message);
}

