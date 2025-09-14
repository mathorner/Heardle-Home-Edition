using Api.LibraryScan;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Hosting;

namespace Api.Tests;

public class LibraryScanServiceTests
{
    private sealed class FakeEnv : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = default!;
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = default!;
    }

    private sealed class FakeSettings : ISettingsService
    {
        private readonly string? _path;
        public FakeSettings(string? path) { _path = path; }
        public (bool Ok, string? Code, string? Message, string? NormalizedPath) ValidatePath(string? input) => (true, null, null, input);
        public Task SaveAsync(string path, CancellationToken ct = default) => Task.CompletedTask;
        public Task<string?> LoadAsync(CancellationToken ct = default) => Task.FromResult(_path);
    }

    private sealed class FakeExtractor : ITrackMetadataExtractor
    {
        public (string Artist, string Title) Extract(string filePath) => ("A", "T");
    }

    private sealed class CollectingWriter : IIndexWriter
    {
        public List<TrackRecord> Written { get; } = new();
        public Task WriteAsync(IEnumerable<TrackRecord> records, IWebHostEnvironment env, CancellationToken ct = default)
        {
            Written.Clear();
            Written.AddRange(records);
            // Write a file too to exercise pathing
            Directory.CreateDirectory(Path.Combine(env.ContentRootPath, "data"));
            File.WriteAllText(Path.Combine(env.ContentRootPath, "data", "library.json"), "[]");
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ScanProducesRecordsAndWritesIndex()
    {
        var root = Directory.CreateTempSubdirectory();
        try
        {
            File.WriteAllText(Path.Combine(root.FullName, "a.mp3"), "");
            File.WriteAllText(Path.Combine(root.FullName, "b.txt"), "");

            var env = new FakeEnv { ContentRootPath = root.FullName };
            var settings = new FakeSettings(root.FullName);
            var extractor = new FakeExtractor();
            var writer = new CollectingWriter();
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<LibraryScanService>.Instance;
            var svc = new LibraryScanService(env, settings, extractor, writer, logger);

            var (total, indexed, failed) = await svc.ScanAsync();
            Assert.Equal(1, indexed);
            Assert.Equal(1, total - failed); // Ensure only mp3 counted as candidate
            Assert.True(File.Exists(Path.Combine(root.FullName, "data", "library.json")));
            Assert.Single(writer.Written);
            Assert.Equal("a.mp3", Path.GetFileName(writer.Written[0].Path));
        }
        finally
        {
            root.Delete(true);
        }
    }
}
