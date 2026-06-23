using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SimpleMCP.Server.Services;

namespace SimpleMCP.Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.InputEncoding = System.Text.Encoding.UTF8;
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(AppContext.BaseDirectory, "logs", "mcp-server-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            await RunAsync(args);
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static async Task RunAsync(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: false);

        builder.Logging.ClearProviders();
        builder.Services.AddSerilog(Log.Logger, dispose: true);

        builder.Services.AddSingleton<McpServer>();

        var host = builder.Build();

        // Startup validation
        try
        {
            await ValidateStartupAsync(host.Services);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Startup validation failed: {ex.Message}");
            return;
        }

        var server = host.Services.GetRequiredService<McpServer>();

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        await host.StartAsync(cts.Token);

        Log.Information("MCP server started");
        await server.RunAsync(cts.Token);
        Log.Information("MCP server stopped");

        await host.StopAsync();
    }

    private static async Task ValidateStartupAsync(IServiceProvider services)
    {
        // Validate any required services or configurations here
    }
}
