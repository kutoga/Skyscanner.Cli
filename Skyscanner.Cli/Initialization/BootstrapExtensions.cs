namespace Skyscanner.Cli.Initialization
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using Skyscanner.Domain.Browser;
    using Skyscanner.Domain.Browser.Delayed;
    using Skyscanner.Domain.Browser.Retry;
    using Skyscanner.Domain.FlightPrice;
    using Skyscanner.Infrastructure.Browser;
    using Skyscanner.Infrastructure.Browser.Retry;

    public static class BootstrapExtensions
    {
        public static IHostBuilder ConfigureFlightPriceDetection(this IHostBuilder hostBuilder, bool headless = true)
        {
            return hostBuilder
                .ConfigureServices(services =>
                {
                    services
                        .AddSingleton<IBrowserActionsRetry, ThreeTimesBrowserActionsRetry>()
                        .AddSingleton<IBrowserFactory>(service =>
                        {
                            return new DelayedBrowserFactory(
                                SeleniumBrowserFactory.GetFirefoxBrowserFactory(service.GetService<ILogger>(), headless),
                                service.GetService<ILogger>());
                        })
                        .AddSingleton<IFlightPriceDetections>(service =>
                        {
                            return new FlightPriceDetections(
                                service.GetService<ILogger>(),
                                ("skyscanner", new SkyscannerFlightPriceDetection(
                                    service.GetService<IBrowserFactory>(),
                                    service.GetService<IBrowserActionsRetry>())));
                        });
                });
        }
    }
}
