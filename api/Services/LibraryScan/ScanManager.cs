using System.Text.Json.Serialization;

namespace Api.LibraryScan;

public interface IScanManager
{
    bool TryStart(Func<CancellationToken, Task<(int total, int indexed, int failed)>> launcher);
    ScanStatusSnapshot GetStatus();
}

public record ScanStatusSnapshot(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("indexed")] int Indexed,
    [property: JsonPropertyName("failed")] int Failed,
    [property: JsonPropertyName("startedAt")] DateTimeOffset? StartedAt,
    [property: JsonPropertyName("finishedAt")] DateTimeOffset? FinishedAt
);

public class ScanManager : IScanManager
{
    private readonly object _gate = new();
    private string _status = "idle";
    private int _total;
    private int _indexed;
    private int _failed;
    private DateTimeOffset? _startedAt;
    private DateTimeOffset? _finishedAt;

    public bool TryStart(Func<CancellationToken, Task<(int total, int indexed, int failed)>> launcher)
    {
        lock (_gate)
        {
            if (_status == "running") return false;
            _status = "running";
            _total = 0;
            _indexed = 0;
            _failed = 0;
            _finishedAt = null;
            _startedAt = DateTimeOffset.UtcNow;
        }

        var cts = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            try
            {
                var (total, indexed, failed) = await launcher(cts.Token);
                lock (_gate)
                {
                    _total = total;
                    _indexed = indexed;
                    _failed = failed;
                    _status = "completed";
                    _finishedAt = DateTimeOffset.UtcNow;
                }
            }
            catch
            {
                lock (_gate)
                {
                    _status = "completed";
                    _finishedAt = DateTimeOffset.UtcNow;
                }
            }
        }, cts.Token);

        return true;
    }

    public ScanStatusSnapshot GetStatus()
    {
        lock (_gate)
        {
            return new ScanStatusSnapshot(_status, _total, _indexed, _failed, _startedAt, _finishedAt);
        }
    }
}

