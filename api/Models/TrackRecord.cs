using System.Text.Json.Serialization;

namespace Api.LibraryScan;

public record TrackRecord(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("artist")] string Artist,
    [property: JsonPropertyName("path")] string Path);

