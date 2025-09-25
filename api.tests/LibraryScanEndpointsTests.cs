using Api.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

public class LibraryScanEndpointsTests
{
    private sealed class SlowScanFactory : TestWebApplicationFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<Api.LibraryScan.ILibraryScanService>(sp => new SlowScanService());
            });
        }
    }

    private sealed class SlowScanService : Api.LibraryScan.ILibraryScanService
    {
        public async Task<(int Total, int Indexed, int Failed)> ScanAsync(CancellationToken ct = default)
        {
            await Task.Delay(300, ct);
            return (10, 9, 1);
        }
    }

    [Fact]
    public async Task Scan_Starts_WhenLibraryPathConfigured_Returns202()
    {
        await using var factory = new SlowScanFactory();
        var client = factory.CreateClient();
        // Configure a valid path
        var dir = Directory.CreateTempSubdirectory();
        try
        {
            var ok = await client.PostAsJsonAsync("/settings/library-path", new { path = dir.FullName });
            Assert.Equal(HttpStatusCode.OK, ok.StatusCode);

            var start = await client.PostAsync("/library/scan", content: null);
            Assert.Equal(HttpStatusCode.Accepted, start.StatusCode);

            var status = await client.GetFromJsonAsync<Dictionary<string, object>>("/library/status");
            Assert.NotNull(status);
            Assert.True(status!["status"]!.ToString() == "running" || status!["status"]!.ToString() == "completed");
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public async Task Scan_Returns409_WhenAlreadyRunning()
    {
        await using var factory = new SlowScanFactory();
        var client = factory.CreateClient();
        var dir = Directory.CreateTempSubdirectory();
        try
        {
            var ok = await client.PostAsJsonAsync("/settings/library-path", new { path = dir.FullName });
            Assert.Equal(HttpStatusCode.OK, ok.StatusCode);

            var start1 = await client.PostAsync("/library/scan", content: null);
            Assert.Equal(HttpStatusCode.Accepted, start1.StatusCode);

            // Immediately attempt another start while first is running
            var start2 = await client.PostAsync("/library/scan", content: null);
            Assert.Equal(HttpStatusCode.Conflict, start2.StatusCode);
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public async Task Scan_Returns400_WhenMissingLibraryPath()
    {
        await using var factory = new SlowScanFactory();
        var client = factory.CreateClient();

        var resp = await client.PostAsync("/library/scan", content: null);
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        var payload = await resp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(payload);
        Assert.Equal("MissingLibraryPath", payload!["code"]?.ToString());
    }
}

