using System.Collections.Concurrent;
using Api.LibraryScan;

namespace Api.Game;

public static class GameSessionStatus
{
    public const string Active = "active";
    public const string Won = "won";
    public const string Lost = "lost";
}

public record GameSession(
    Guid Id,
    string TrackId,
    string TrackPath,
    string Title,
    string Artist,
    int Attempt,
    int MaxAttempts,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public interface IGameSessionStore
{
    GameSession Create(TrackRecord track, int maxAttempts = 6);
    bool TryGet(Guid id, out GameSession? session);
    bool TryUpdate(Guid id, Func<GameSession, GameSession> updater, out GameSession? updated);
}

/// <summary>
/// Thread-safe, single-node session cache that keeps active game sessions in
/// memory and expires them after a configurable idle window. Designed for the
/// home-edition use case where only one player hits the server at a time.
/// </summary>
public sealed class InMemoryGameSessionStore : IGameSessionStore
{
    private readonly ILogger<InMemoryGameSessionStore> _logger;
    private readonly TimeSpan _expiration;
    private readonly Func<DateTimeOffset> _nowProvider;
    private readonly ConcurrentDictionary<Guid, StoredSession> _sessions = new();

    private sealed record StoredSession(GameSession Session);

    public InMemoryGameSessionStore(
        ILogger<InMemoryGameSessionStore> logger,
        TimeSpan? expiration = null,
        Func<DateTimeOffset>? nowProvider = null)
    {
        _logger = logger;
        _expiration = expiration ?? TimeSpan.FromHours(2);
        _nowProvider = nowProvider ?? (() => DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Create a new session for the supplied track and mark it active. Any
    /// expired sessions are purged as part of the operation so the store stays
    /// bounded without a dedicated background worker.
    /// </summary>
    public GameSession Create(TrackRecord track, int maxAttempts = 6)
    {
        var now = _nowProvider();
        Sweep(now);

        var session = new GameSession(
            Guid.NewGuid(),
            track.Id,
            track.Path,
            track.Title,
            track.Artist,
            Attempt: 1,
            MaxAttempts: maxAttempts,
            Status: GameSessionStatus.Active,
            CreatedAt: now,
            UpdatedAt: now);

        _sessions[session.Id] = new StoredSession(session);
        _logger.LogInformation("Created game session {SessionId} for track {TrackId}", session.Id, track.Id);
        return session;
    }

    /// <summary>
    /// Try to look up a session by id. Expired entries are evicted before the
    /// lookup to ensure callers never receive stale state.
    /// </summary>
    public bool TryGet(Guid id, out GameSession? session)
    {
        var now = _nowProvider();
        Sweep(now);

        if (_sessions.TryGetValue(id, out var existing) && !IsExpired(existing.Session, now))
        {
            session = existing.Session;
            return true;
        }

        session = null;
        _sessions.TryRemove(id, out _);
        return false;
    }

    /// <summary>
    /// Atomically mutate the stored session. Updates fail when the session has
    /// expired or is missing. Successful mutations refresh the UpdatedAt stamp.
    /// </summary>
    public bool TryUpdate(Guid id, Func<GameSession, GameSession> updater, out GameSession? updated)
    {
        var now = _nowProvider();
        Sweep(now);

        while (true)
        {
            if (!_sessions.TryGetValue(id, out var existing) || IsExpired(existing.Session, now))
            {
                _sessions.TryRemove(id, out _);
                updated = null;
                return false;
            }

            var next = updater(existing.Session) with { UpdatedAt = now };
            var stored = new StoredSession(next);

            if (_sessions.TryUpdate(id, stored, existing))
            {
                updated = next;
                return true;
            }
        }
    }

    /// <summary>
    /// Remove sessions whose <see cref="GameSession.UpdatedAt"/> is older than
    /// the configured expiration window. Called opportunistically before each
    /// public operation instead of running a background timer.
    /// </summary>
    private void Sweep(DateTimeOffset now)
    {
        var removed = 0;
        foreach (var kvp in _sessions)
        {
            if (IsExpired(kvp.Value.Session, now) && _sessions.TryRemove(kvp.Key, out _))
            {
                removed++;
            }
        }

        if (removed > 0)
        {
            _logger.LogDebug("Removed {Count} expired game sessions", removed);
        }
    }

    private bool IsExpired(GameSession session, DateTimeOffset now) =>
        now - session.UpdatedAt >= _expiration;
}
