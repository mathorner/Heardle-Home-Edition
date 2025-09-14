using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

/// <summary>
/// Handles validation and persistence of application settings that live on the server.
/// For now we only manage <c>libraryPath</c> for the home-edition use case.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Validate a user-supplied path. Requires an absolute, existing, and minimally readable directory.
    /// Returns a normalized absolute path when valid along with an error code/message when invalid.
    /// </summary>
    (bool Ok, string? Code, string? Message, string? NormalizedPath) ValidatePath(string? input);

    /// <summary>
    /// Persist the validated library path to <c>config/settings.json</c> using an atomic write.
    /// </summary>
    Task SaveAsync(string path, CancellationToken ct = default);

    /// <summary>
    /// Load the saved library path from <c>config/settings.json</c>, or <c>null</c> if not present/invalid.
    /// </summary>
    Task<string?> LoadAsync(CancellationToken ct = default);
}

/// <summary>
/// File-backed settings provider that validates and persists the library path to
/// {ContentRoot}/config/settings.json using JSON with camelCase keys and atomic writes.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<SettingsService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SettingsService(IWebHostEnvironment env, ILogger<SettingsService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public (bool Ok, string? Code, string? Message, string? NormalizedPath) ValidatePath(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return (false, "InvalidPath", "Path is required.", null);
        }

        var raw = input.Trim();
        // Reject relative inputs BEFORE normalization per spec. Calling GetFullPath on a relative
        // path produces an absolute path which would mask the error and misclassify as NotFound.
        if (!Path.IsPathRooted(raw))
        {
            return (false, "InvalidPath", "Path must be absolute.", null);
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(raw);
        }
        catch
        {
            return (false, "InvalidPath", "Path format is invalid.", null);
        }

        if (!Directory.Exists(fullPath))
        {
            return (false, "NotFound", "Directory does not exist.", null);
        }

        try
        {
            // Attempt to actually read the directory (minimal touch) to detect permission issues.
            using var e = Directory.EnumerateFileSystemEntries(fullPath).Take(1).GetEnumerator();
            e.MoveNext();
        }
        catch (UnauthorizedAccessException)
        {
            return (false, "AccessDenied", "Directory is not readable by the server.", null);
        }
        catch
        {
            return (false, "Unknown", "Unable to access directory.", null);
        }

        return (true, null, null, fullPath);
    }

    public async Task SaveAsync(string path, CancellationToken ct = default)
    {
        var configDir = Path.Combine(_env.ContentRootPath, "config");
        Directory.CreateDirectory(configDir);
        var settingsPath = Path.Combine(configDir, "settings.json");

        var payload = new SettingsFile(path);
        var json = JsonSerializer.Serialize(payload, JsonOptions);

        // Atomic write: write to a temp file and then replace/move into place
        var tmp = settingsPath + ".tmp";
        await File.WriteAllTextAsync(tmp, json, ct);
        if (File.Exists(settingsPath))
        {
            File.Replace(tmp, settingsPath, settingsPath + ".bak", ignoreMetadataErrors: true);
        }
        else
        {
            File.Move(tmp, settingsPath);
        }

        _logger.LogInformation("Saved library path to configuration");
    }

    private record SettingsFile([property: JsonPropertyName("libraryPath")] string LibraryPath);

    public async Task<string?> LoadAsync(CancellationToken ct = default)
    {
        var configDir = Path.Combine(_env.ContentRootPath, "config");
        var settingsPath = Path.Combine(configDir, "settings.json");
        if (!File.Exists(settingsPath)) return null;
        try
        {
            await using var fs = File.OpenRead(settingsPath);
            var settings = await JsonSerializer.DeserializeAsync<SettingsFile>(fs, JsonOptions, ct);
            return settings?.LibraryPath;
        }
        catch
        {
            // Corrupt file or invalid json; treat as missing and let the UI prompt the user again.
            return null;
        }
    }
}
