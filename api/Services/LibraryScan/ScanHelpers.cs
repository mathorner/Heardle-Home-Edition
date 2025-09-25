using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Api.LibraryScan;

/// <summary>
/// Utility methods used by the library scanner for normalization, filename parsing,
/// deterministic ID generation, and safe streaming directory enumeration.
/// </summary>
public static class ScanHelpers
{
    private static readonly Regex MultiSpace = new(@"\s+", RegexOptions.Compiled);
    private static readonly HashSet<string> SupportedAudioExtensions = new(
        new[] { ".mp3", ".m4a", ".aac", ".flac", ".wav", ".ogg", ".wma", ".aiff", ".aif" },
        StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Trim and collapse all whitespace (including tabs/newlines) into single spaces.
    /// </summary>
    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var trimmed = input.Trim();
        // Collapse whitespace (newlines, tabs, multiple spaces) to single spaces
        return MultiSpace.Replace(trimmed, " ").Trim();
    }

    /// <summary>
    /// Best-effort title/artist extraction from a filename following the common
    /// "Artist - Title.mp3" pattern. Falls back to title-only when no separator present.
    /// </summary>
    public static (string Artist, string Title) ParseFromFilename(string path)
    {
        var name = System.IO.Path.GetFileNameWithoutExtension(path);
        // Common convention: "Artist - Title"
        var parts = name.Split(" - ", 2, StringSplitOptions.TrimEntries);
        if (parts.Length == 2)
        {
            return (Normalize(parts[0]), Normalize(parts[1]));
        }
        // Fallback: no artist, title from filename
        return (string.Empty, Normalize(name));
    }

    /// <summary>
    /// Generate a stable, deterministic ID from the absolute file path.
    /// Uses SHA-256(hex) truncated to 32 characters (16 bytes) for brevity.
    /// </summary>
    public static string DeterministicIdFromPath(string absolutePath)
    {
        // SHA-256 hex truncated to 32 chars (16 bytes)
        var bytes = Encoding.UTF8.GetBytes(absolutePath);
        var hash = SHA256.HashData(bytes);
        var hex = Convert.ToHexString(hash).ToLowerInvariant();
        return hex.Substring(0, 32);
    }

    /// <summary>
    /// Depth-first streaming directory enumeration that yields supported audio files
    /// (case-insensitive) and best-effort skips of hidden/system directories. Swallows
    /// per-directory exceptions to keep progressing through large/partially
    /// inaccessible trees.
    /// </summary>
    public static IEnumerable<string> EnumerateAudioFiles(string root)
    {
        var stack = new Stack<string>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var dir = stack.Pop();
            IEnumerable<string> subdirs;
            try
            {
                subdirs = Directory.EnumerateDirectories(dir);
            }
            catch
            {
                continue;
            }

            foreach (var sd in subdirs)
            {
                try
                {
                    var info = new DirectoryInfo(sd);
                    if (info.Name.StartsWith('.') || info.Attributes.HasFlag(FileAttributes.Hidden) || info.Attributes.HasFlag(FileAttributes.System))
                    {
                        continue;
                    }
                    stack.Push(sd);
                }
                catch
                {
                    stack.Push(sd);
                }
            }

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(dir);
            }
            catch
            {
                continue;
            }

            foreach (var f in files)
            {
                var extension = Path.GetExtension(f);
                if (extension is { Length: > 0 } && SupportedAudioExtensions.Contains(extension))
                {
                    yield return f;
                }
            }
        }
    }
}
