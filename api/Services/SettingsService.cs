using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

public interface ISettingsService
{
    (bool Ok, string? Code, string? Message, string? NormalizedPath) ValidatePath(string? input);
    Task SaveAsync(string path, CancellationToken ct = default);
}

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
        // Reject relative inputs before normalization per spec
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
            // Attempt to actually read the directory (minimal touch)
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
}
