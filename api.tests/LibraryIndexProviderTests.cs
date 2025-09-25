using System.Text.Json;
using Api.LibraryIndex;
using Api.LibraryScan;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;

namespace Api.Tests;

public class LibraryIndexProviderTests
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

    [Fact]
    public async Task GetAsync_LoadsTracksAndSkipsMissingFiles()
    {
        var root = Directory.CreateTempSubdirectory();
        try
        {
            var dataDir = Path.Combine(root.FullName, "data");
            Directory.CreateDirectory(dataDir);

            var existingPath = Path.Combine(root.FullName, "keep.mp3");
            File.WriteAllText(existingPath, string.Empty);

            var missingPath = Path.Combine(root.FullName, "missing.mp3");

            var records = new[]
            {
                new TrackRecord("one", "Song", "Artist", existingPath),
                new TrackRecord("two", "Other", "Artist", missingPath)
            };
            await File.WriteAllTextAsync(
                Path.Combine(dataDir, "library.json"),
                JsonSerializer.Serialize(records));

            var env = new FakeEnv { ContentRootPath = root.FullName };
            var provider = new LibraryIndexProvider(env, NullLogger<LibraryIndexProvider>.Instance);

            var snapshot = await provider.GetAsync();

            Assert.Equal(LibraryIndexStatus.Ready, snapshot.Status);
            Assert.Equal(1, snapshot.TotalTracks);
            Assert.Single(snapshot.Tracks);
            Assert.Equal("one", snapshot.Tracks[0].Id);
            Assert.NotNull(snapshot.LastLoadedAt);
            Assert.Null(snapshot.ErrorCode);
        }
        finally
        {
            root.Delete(true);
        }
    }

    [Fact]
    public async Task GetAsync_ReturnsLibraryNotReadyWhenIndexMissing()
    {
        var root = Directory.CreateTempSubdirectory();
        try
        {
            var env = new FakeEnv { ContentRootPath = root.FullName };
            var provider = new LibraryIndexProvider(env, NullLogger<LibraryIndexProvider>.Instance);

            var snapshot = await provider.GetAsync();

            Assert.Equal(LibraryIndexStatus.NotReady, snapshot.Status);
            Assert.Equal(LibraryIndexErrorCodes.LibraryNotReady, snapshot.ErrorCode);
            Assert.Empty(snapshot.Tracks);
            Assert.Equal(0, snapshot.TotalTracks);
        }
        finally
        {
            root.Delete(true);
        }
    }

    [Fact]
    public async Task GetAsync_ReturnsNoTracksIndexedWhenAllMissing()
    {
        var root = Directory.CreateTempSubdirectory();
        try
        {
            var dataDir = Path.Combine(root.FullName, "data");
            Directory.CreateDirectory(dataDir);

            var records = new[]
            {
                new TrackRecord("one", "Song", "Artist", Path.Combine(root.FullName, "missing1.mp3"))
            };
            await File.WriteAllTextAsync(
                Path.Combine(dataDir, "library.json"),
                JsonSerializer.Serialize(records));

            var env = new FakeEnv { ContentRootPath = root.FullName };
            var provider = new LibraryIndexProvider(env, NullLogger<LibraryIndexProvider>.Instance);

            var snapshot = await provider.GetAsync();

            Assert.Equal(LibraryIndexStatus.Empty, snapshot.Status);
            Assert.Equal(LibraryIndexErrorCodes.NoTracksIndexed, snapshot.ErrorCode);
            Assert.Empty(snapshot.Tracks);
            Assert.Equal(0, snapshot.TotalTracks);
        }
        finally
        {
            root.Delete(true);
        }
    }

    [Fact]
    public async Task GetAsync_RefreshesCacheWhenTimestampChanges()
    {
        var root = Directory.CreateTempSubdirectory();
        try
        {
            var dataDir = Path.Combine(root.FullName, "data");
            Directory.CreateDirectory(dataDir);

            var trackOnePath = Path.Combine(root.FullName, "one.mp3");
            File.WriteAllText(trackOnePath, string.Empty);

            async Task WriteRecordsAsync(params TrackRecord[] records)
            {
                var json = JsonSerializer.Serialize(records);
                await File.WriteAllTextAsync(Path.Combine(dataDir, "library.json"), json);
            }

            await WriteRecordsAsync(new TrackRecord("one", "Song", "Artist", trackOnePath));

            var env = new FakeEnv { ContentRootPath = root.FullName };
            var provider = new LibraryIndexProvider(env, NullLogger<LibraryIndexProvider>.Instance);

            var first = await provider.GetAsync();
            Assert.Equal(1, first.TotalTracks);

            var trackTwoPath = Path.Combine(root.FullName, "two.mp3");
            File.WriteAllText(trackTwoPath, string.Empty);
            await WriteRecordsAsync(
                new TrackRecord("one", "Song", "Artist", trackOnePath),
                new TrackRecord("two", "Next", "Artist", trackTwoPath));

            File.SetLastWriteTimeUtc(Path.Combine(dataDir, "library.json"), DateTime.UtcNow.AddMinutes(1));

            var second = await provider.GetAsync();

            Assert.Equal(2, second.TotalTracks);
            Assert.Equal(LibraryIndexStatus.Ready, second.Status);
        }
        finally
        {
            root.Delete(true);
        }
    }
}

