namespace Skyscanner.Cli.Initialization
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Exceptions;
    using Serilog.Formatting.Compact;

    public static class LoggingExtensions
    {
        public static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureLogging((hostContext, logging) =>
                {
                    logging.ClearProviders();
                })
                .UseSerilog((hostContext, loggingConfig) =>
                {
                    loggingConfig
                        .ReadFrom.Configuration(hostContext.Configuration)
                        .WriteTo.Console(new CompactJsonFormatter())
                        .Enrich.FromLogContext()
                        .Enrich.WithExceptionDetails()
                        .MinimumLevel.Debug();
                })
                .ConfigureServices(services => services.AddSingleton<Serilog.ILogger>(Log.Logger));
        }
    }
}
