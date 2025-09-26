using Api.Game;
using Api.LibraryScan;
using Microsoft.Extensions.Logging.Abstractions;

namespace Api.Tests;

public class GameSessionStoreTests
{
    private sealed class TestClock
    {
        private DateTimeOffset _now;
        public TestClock(DateTimeOffset start) => _now = start;
        public DateTimeOffset UtcNow => _now;
        public void Advance(TimeSpan delta) => _now += delta;
    }

    private static TrackRecord MakeTrack(string id = "track-1") =>
        new(id, "Song", "Artist", $"/music/{id}.mp3");

    [Fact]
    public void CreateAddsSessionWithDefaults()
    {
        var clock = new TestClock(DateTimeOffset.Parse("2025-09-25T12:00:00Z"));
        var store = new InMemoryGameSessionStore(NullLogger<InMemoryGameSessionStore>.Instance, nowProvider: () => clock.UtcNow);

        var session = store.Create(MakeTrack());

        Assert.NotEqual(Guid.Empty, session.Id);
        Assert.Equal("track-1", session.TrackId);
        Assert.Equal("/music/track-1.mp3", session.TrackPath);
        Assert.Equal("Song", session.Title);
        Assert.Equal("Artist", session.Artist);
        Assert.Equal(1, session.Attempt);
        Assert.Equal(6, session.MaxAttempts);
        Assert.Equal(GameSessionStatus.Active, session.Status);
        Assert.Equal(clock.UtcNow, session.CreatedAt);
        Assert.Equal(clock.UtcNow, session.UpdatedAt);

        Assert.True(store.TryGet(session.Id, out var fetched));
        Assert.Equal(session, fetched);
    }

    [Fact]
    public void TryUpdateMutatesSession()
    {
        var clock = new TestClock(DateTimeOffset.Parse("2025-09-25T12:00:00Z"));
        var store = new InMemoryGameSessionStore(NullLogger<InMemoryGameSessionStore>.Instance, nowProvider: () => clock.UtcNow);
        var session = store.Create(MakeTrack());

        clock.Advance(TimeSpan.FromMinutes(5));
        var updated = store.TryUpdate(session.Id, s => s with { Attempt = s.Attempt + 1, Status = GameSessionStatus.Won }, out var next);

        Assert.True(updated);
        Assert.NotNull(next);
        Assert.Equal(2, next!.Attempt);
        Assert.Equal(GameSessionStatus.Won, next.Status);
        Assert.Equal(clock.UtcNow, next.UpdatedAt);
        Assert.True(store.TryGet(session.Id, out var fetched));
        Assert.Equal(next, fetched);
    }

    [Fact]
    public void ExpiredSessionsAreSwept()
    {
        var clock = new TestClock(DateTimeOffset.Parse("2025-09-25T12:00:00Z"));
        var store = new InMemoryGameSessionStore(
            NullLogger<InMemoryGameSessionStore>.Instance,
            expiration: TimeSpan.FromHours(2),
            nowProvider: () => clock.UtcNow);
        var session = store.Create(MakeTrack());

        clock.Advance(TimeSpan.FromHours(3));

        Assert.False(store.TryGet(session.Id, out _));

        // Subsequent create should not resurrect expired session
        var session2 = store.Create(MakeTrack("track-2"));
        Assert.True(store.TryGet(session2.Id, out _));
        Assert.False(store.TryGet(session.Id, out _));
    }

    [Fact]
    public void TryUpdateReturnsFalseWhenSessionMissingOrExpired()
    {
        var clock = new TestClock(DateTimeOffset.Parse("2025-09-25T12:00:00Z"));
        var store = new InMemoryGameSessionStore(
            NullLogger<InMemoryGameSessionStore>.Instance,
            expiration: TimeSpan.FromHours(1),
            nowProvider: () => clock.UtcNow);
        var session = store.Create(MakeTrack());

        clock.Advance(TimeSpan.FromHours(2));

        var updated = store.TryUpdate(session.Id, s => s with { Attempt = s.Attempt + 1 }, out var next);
        Assert.False(updated);
        Assert.Null(next);
    }
}
