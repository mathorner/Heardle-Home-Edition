using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.LibraryScan;

/// <summary>
/// Writes the scanned music library index to disk in a durable, JSON format.
/// The default implementation persists to {ContentRoot}/data/library.json using
/// camelCase fields and an atomic write (temp file + replace/move) to avoid
/// partial writes and ensure consistency even on failures.
/// </summary>
public interface IIndexWriter
{
    Task WriteAsync(IEnumerable<TrackRecord> records, IWebHostEnvironment env, CancellationToken ct = default);
}

/// <summary>
/// JSON index writer that serializes <see cref="TrackRecord"/> entries to
/// {ContentRoot}/data/library.json with camelCase fields and pretty printing.
/// </summary>
public class JsonIndexWriter : IIndexWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task WriteAsync(IEnumerable<TrackRecord> records, IWebHostEnvironment env, CancellationToken ct = default)
    {
        // The index lives under {ContentRoot}/data/library.json
        var dataDir = Path.Combine(env.ContentRootPath, "data");
        Directory.CreateDirectory(dataDir);
        var indexPath = Path.Combine(dataDir, "library.json");

        // Atomic write: write to a temp file first, then replace/move to avoid partial writes
        var tmp = indexPath + ".tmp";
        await using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await JsonSerializer.SerializeAsync(fs, records, JsonOptions, ct);
        }

        if (File.Exists(indexPath))
        {
            File.Replace(tmp, indexPath, indexPath + ".bak", ignoreMetadataErrors: true);
        }
        else
        {
            File.Move(tmp, indexPath);
        }
    }
}
