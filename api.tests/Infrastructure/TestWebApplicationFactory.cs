using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Tests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly DirectoryInfo _tempRoot;

    public TestWebApplicationFactory()
    {
        _tempRoot = Directory.CreateTempSubdirectory();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(_tempRoot.FullName);
        base.ConfigureWebHost(builder);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try
        {
            if (_tempRoot.Exists)
            {
                _tempRoot.Delete(true);
            }
        }
        catch
        {
            // ignore cleanup errors
        }
    }
}

