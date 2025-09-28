using System.Security.Cryptography;
using Api.LibraryScan;

namespace Api.Game;

public interface IRandomTrackSelector
{
    TrackRecord? TrySelect(IReadOnlyList<TrackRecord> tracks);
}

public sealed class RandomTrackSelector : IRandomTrackSelector
{
    private readonly object _gate = new();
    private string? _lastTrackId;

    public TrackRecord? TrySelect(IReadOnlyList<TrackRecord> tracks)
    {
        if (tracks is null || tracks.Count == 0) return null;
        if (tracks.Count == 1)
        {
            var only = tracks[0];
            lock (_gate)
            {
                _lastTrackId = only.Id;
            }
            return only;
        }

        lock (_gate)
        {
            for (var attempts = 0; attempts < 3; attempts++)
            {
                var index = RandomNumberGenerator.GetInt32(tracks.Count);
                var candidate = tracks[index];
                if (candidate.Id != _lastTrackId)
                {
                    _lastTrackId = candidate.Id;
                    return candidate;
                }
            }

            var fallback = tracks[RandomNumberGenerator.GetInt32(tracks.Count)];
            _lastTrackId = fallback.Id;
            return fallback;
        }
    }
}

