using Api.LibraryIndex;

namespace Api.LibraryScan;

/// <summary>
/// Orchestrates a full library scan: enumerates audio files under the configured
/// library path, extracts metadata via <see cref="ITrackMetadataExtractor"/>,
/// generates deterministic IDs, and persists the resulting index via <see cref="IIndexWriter"/>.
/// Continues past per-file errors and logs a completion summary.
/// </summary>
public interface ILibraryScanService
{
    Task<(int Total, int Indexed, int Failed)> ScanAsync(CancellationToken ct = default);
}

/// <summary>
/// Default implementation of <see cref="ILibraryScanService"/> that streams directory
/// traversal, handles large libraries efficiently, and writes a stable, camelCase JSON index.
/// </summary>
public class LibraryScanService : ILibraryScanService
{
    private readonly IWebHostEnvironment _env;
    private readonly ISettingsService _settings;
    private readonly ITrackMetadataExtractor _extractor;
    private readonly IIndexWriter _writer;
    private readonly ILibraryIndexProvider _indexProvider;
    private readonly ILogger<LibraryScanService> _logger;

    public LibraryScanService(
        IWebHostEnvironment env,
        ISettingsService settings,
        ITrackMetadataExtractor extractor,
        IIndexWriter writer,
        ILibraryIndexProvider indexProvider,
        ILogger<LibraryScanService> logger)
    {
        _env = env;
        _settings = settings;
        _extractor = extractor;
        _writer = writer;
        _indexProvider = indexProvider;
        _logger = logger;
    }

    /// <summary>
    /// Run a full scan using the configured library path, producing a JSON index.
    /// Continues past per-file errors and logs a summary on completion.
    /// </summary>
    public async Task<(int Total, int Indexed, int Failed)> ScanAsync(CancellationToken ct = default)
    {
        var root = await _settings.LoadAsync(ct);
        if (string.IsNullOrWhiteSpace(root))
        {
            _logger.LogWarning("Library path not configured; skipping scan");
            return (0, 0, 0);
        }

        var total = 0;
        var indexed = 0;
        var failed = 0;
        var list = new List<TrackRecord>(capacity: 1024);

        foreach (var file in ScanHelpers.EnumerateAudioFiles(root))
        {
            ct.ThrowIfCancellationRequested();
            total++;
            try
            {
                var (artist, title) = _extractor.Extract(file);
                var id = ScanHelpers.DeterministicIdFromPath(file);
                list.Add(new TrackRecord(id, title, artist, file));
                indexed++;
            }
            catch (Exception ex)
            {
                failed++;
                _logger.LogWarning(ex, "Failed to index file: {File}", file);
            }
        }

        // Optional stable ordering for human-readable diffs (artist, then title)
        list.Sort((a, b) => string.Compare(a.Artist, b.Artist, StringComparison.OrdinalIgnoreCase) switch
        {
            0 => string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase),
            var c => c
        });

        await _writer.WriteAsync(list, _env, ct);
        _indexProvider.Invalidate();
        _logger.LogInformation("Scan completed. Total: {Total}, Indexed: {Indexed}, Failed: {Failed}", total, indexed, failed);
        return (total, indexed, failed);
    }
}
