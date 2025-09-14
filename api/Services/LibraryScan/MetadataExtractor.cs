using TagLib;

namespace Api.LibraryScan;

/// <summary>
/// Contract for extracting artist/title metadata from an audio file path.
/// Implementations should prefer tag-based extraction with a filename fallback.
/// </summary>
public interface ITrackMetadataExtractor
{
    (string Artist, string Title) Extract(string filePath);
}

/// <summary>
/// TagLib#-based metadata extractor that reads ID3 tags for title and artist,
/// normalizes whitespace, and falls back to filename parsing on missing or
/// unreadable metadata.
/// </summary>
public class TagLibMetadataExtractor : ITrackMetadataExtractor
{
    public (string Artist, string Title) Extract(string filePath)
    {
        try
        {
            // TagLib reads metadata without fully decoding audio; suitable for quick tag lookups
            using var file = TagLib.File.Create(filePath);
            var title = ScanHelpers.Normalize(file.Tag.Title);
            string artist = string.Empty;
            if (file.Tag.Performers is { Length: > 0 })
            {
                artist = ScanHelpers.Normalize(file.Tag.Performers[0]);
            }

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(artist))
            {
                // Fallback to filename-derived metadata when tag data is incomplete
                var fallback = ScanHelpers.ParseFromFilename(filePath);
                title = string.IsNullOrEmpty(title) ? fallback.Title : title;
                artist = string.IsNullOrEmpty(artist) ? fallback.Artist : artist;
            }
            return (artist, title);
        }
        catch
        {
            // On any failure (corrupt/unreadable), fall back to filename parsing
            return ScanHelpers.ParseFromFilename(filePath);
        }
    }
}
