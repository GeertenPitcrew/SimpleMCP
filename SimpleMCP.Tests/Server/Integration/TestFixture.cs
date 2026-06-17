using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleMCP.Server.Services;

namespace SimpleMCP.Tests.Server.Integration;

public class TestFixture : IAsyncLifetime
{
    public IServiceProvider Services { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddSingleton<McpServer>();
        services.AddSingleton<IConfiguration>(configuration);

        Services = services.BuildServiceProvider();
    }

    public async ValueTask DisposeAsync()
    {
        if (Services is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
