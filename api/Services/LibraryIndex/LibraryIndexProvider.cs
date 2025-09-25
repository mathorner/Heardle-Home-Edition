using System.Text.Json;
using Api.LibraryScan;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Api.LibraryIndex;

/// <summary>
/// Provides cached access to the on-disk library index, normalising the
/// snapshot into a status + track list that downstream services can rely on.
/// </summary>
public interface ILibraryIndexProvider
{
    Task<LibraryIndexSnapshot> GetAsync(CancellationToken ct = default);
    void Invalidate();
}

public static class LibraryIndexStatus
{
    public const string Ready = "ready";
    public const string NotReady = "not-ready";
    public const string Empty = "empty";
}

public static class LibraryIndexErrorCodes
{
    public const string LibraryNotReady = nameof(LibraryNotReady);
    public const string NoTracksIndexed = nameof(NoTracksIndexed);
}

/// <summary>
/// Snapshot returned to callers describing the current readiness of the
/// library index alongside the filtered track list.
/// </summary>
public record LibraryIndexSnapshot(
    string Status,
    IReadOnlyList<TrackRecord> Tracks,
    int TotalTracks,
    DateTimeOffset? LastLoadedAt,
    string? ErrorCode,
    string? ErrorMessage);

/// <summary>
/// Loads <c>data/library.json</c>, filters unusable entries, and caches the
/// result until the file timestamp changes or the cache is explicitly invalidated.
/// </summary>
public class LibraryIndexProvider : ILibraryIndexProvider
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<LibraryIndexProvider> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private readonly object _gate = new();
    private LibraryIndexSnapshot? _cached;
    private DateTime? _cachedLastWrite;
    private bool _invalidated;

    public LibraryIndexProvider(IWebHostEnvironment env, ILogger<LibraryIndexProvider> logger)
    {
        _env = env;
        _logger = logger;
    }

    public void Invalidate()
    {
        lock (_gate)
        {
            _invalidated = true;
        }
    }

    public async Task<LibraryIndexSnapshot> GetAsync(CancellationToken ct = default)
    {
        var indexPath = Path.Combine(_env.ContentRootPath, "data", "library.json");
        var fileExists = File.Exists(indexPath);
        var currentWrite = fileExists ? File.GetLastWriteTimeUtc(indexPath) : (DateTime?)null;

        lock (_gate)
        {
            if (!_invalidated && _cached is not null && _cachedLastWrite == currentWrite)
            {
                return _cached;
            }
        }

        var snapshot = await LoadSnapshotAsync(indexPath, fileExists, ct);

        lock (_gate)
        {
            _cached = snapshot;
            _cachedLastWrite = fileExists ? currentWrite : null;
            _invalidated = false;
            return snapshot;
        }
    }

    private async Task<LibraryIndexSnapshot> LoadSnapshotAsync(string indexPath, bool fileExists, CancellationToken ct)
    {
        if (!fileExists)
        {
            return new LibraryIndexSnapshot(
                LibraryIndexStatus.NotReady,
                Array.Empty<TrackRecord>(),
                0,
                null,
                LibraryIndexErrorCodes.LibraryNotReady,
                "Library index not found.");
        }

        try
        {
            await using var stream = File.Open(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var records = await JsonSerializer.DeserializeAsync<List<TrackRecord>>(stream, JsonOptions, ct) ?? new List<TrackRecord>();

            var available = new List<TrackRecord>(records.Count);
            foreach (var record in records)
            {
                ct.ThrowIfCancellationRequested();
                if (File.Exists(record.Path))
                {
                    available.Add(record);
                }
                else
                {
                    _logger.LogInformation("Skipping track with missing file: {Path}", record.Path);
                }
            }

            if (available.Count == 0)
            {
                return new LibraryIndexSnapshot(
                    LibraryIndexStatus.Empty,
                    Array.Empty<TrackRecord>(),
                    0,
                    DateTimeOffset.UtcNow,
                    LibraryIndexErrorCodes.NoTracksIndexed,
                    "No tracks were available. Run a scan to refresh the index.");
            }

            return new LibraryIndexSnapshot(
                LibraryIndexStatus.Ready,
                available,
                available.Count,
                DateTimeOffset.UtcNow,
                null,
                null);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse library index at {Path}", indexPath);
            return new LibraryIndexSnapshot(
                LibraryIndexStatus.NotReady,
                Array.Empty<TrackRecord>(),
                0,
                null,
                LibraryIndexErrorCodes.LibraryNotReady,
                "Library index is corrupt. Rescan your library.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Failed to read library index at {Path}", indexPath);
            return new LibraryIndexSnapshot(
                LibraryIndexStatus.NotReady,
                Array.Empty<TrackRecord>(),
                0,
                null,
                LibraryIndexErrorCodes.LibraryNotReady,
                "Unable to read library index. Ensure the file is accessible.");
        }
    }
}
