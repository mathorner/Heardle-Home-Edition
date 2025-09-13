using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Tests;

public class LibraryPathEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public LibraryPathEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostLibraryPath_InvalidBody_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/settings/library-path", new { path = "" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(payload);
        Assert.Equal("InvalidPath", payload!["code"]?.ToString());
    }

    [Fact]
    public async Task PostLibraryPath_NonAbsolute_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/settings/library-path", new { path = "relative/path" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(payload);
        Assert.Equal("InvalidPath", payload!["code"]?.ToString());
    }

    [Fact]
    public async Task PostLibraryPath_ValidExistingDir_ReturnsOk()
    {
        // Use a temp directory that exists and is readable
        var tempDir = Directory.CreateTempSubdirectory();
        try
        {
            var response = await _client.PostAsJsonAsync("/settings/library-path", new { path = tempDir.FullName });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            Assert.NotNull(payload);
            Assert.Equal("True", payload!["saved"]?.ToString());
            Assert.Equal(tempDir.FullName, payload!["path"]?.ToString());
        }
        finally
        {
            tempDir.Delete(true);
        }
    }
}

