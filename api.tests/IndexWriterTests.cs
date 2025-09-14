using Api.LibraryScan;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Hosting;

namespace Api.Tests;

public class IndexWriterTests
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
    public async Task WritesCamelCaseJsonAtomically()
    {
        var tmp = Directory.CreateTempSubdirectory();
        try
        {
            var env = new FakeEnv { ContentRootPath = tmp.FullName };
            var writer = new JsonIndexWriter();
            var records = new[]
            {
                new TrackRecord("abc123", "Song", "Artist", "/x/y.mp3")
            };

            await writer.WriteAsync(records, env);

            var jsonPath = Path.Combine(tmp.FullName, "data", "library.json");
            Assert.True(File.Exists(jsonPath));
            var txt = await File.ReadAllTextAsync(jsonPath);
            Assert.Contains("\"id\":", txt);
            Assert.Contains("\"title\":", txt);
            Assert.Contains("\"artist\":", txt);
            Assert.Contains("\"path\":", txt);
        }
        finally
        {
            tmp.Delete(true);
        }
    }
}
