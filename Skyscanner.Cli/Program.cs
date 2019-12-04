namespace Skyscanner.Cli
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentArgs;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Skyscanner.Cli.Initialization;
    using Skyscanner.Domain.FlightPrice;
    using Skyscanner.Model;

    public static class Program
    {
        private static bool Headless =>
#if DEBUG
                            false
#else
                            true
#endif
                            ;

        public static Task Main(string[] args)
        {
            args = new[] { "-o", "ZRH", "-d", "HKG", "-t", "2020-01-29" };
            return CreateHostBuilder(args)
                 .Build()
                 .RunAsync();
        }

        private static void ListAirports()
        {
            Console.WriteLine("Available Airports:");
            var sortedAirportNames = Enum.GetValues(typeof(Airport))
                .Cast<Airport>()
                .Select(v => v.ToString())
                .OrderBy(v => v);
            Console.WriteLine(string.Join(", ", sortedAirportNames));
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            IHostBuilder? result = default;
            var builder = FluentArgsBuilder.New()
                .DefaultConfigsWithAppDescription("This tool can be used to query minimum daily prices from skyscanner.")
                .Given.Flag("-a", "--airports").Then(ListAirports)
                .Parameter<Airport>("-o", "--origin")
                    .WithDescription("Origin airport.")
                    .WithExamples(Airport.ZRH, Airport.HKG)
                    .IsRequired()
                .Parameter<Airport>("-d", "--destination")
                    .WithDescription("Destination airport.")
                    .WithExamples(Airport.ZRH, Airport.HKG)
                    .IsRequired()
                .Parameter<DateTimeOffset>("-t", "--date")
                    .WithDescription("Flight date.")
                    .IsRequired()
                .Call(date => destination => origin =>
                {
                    result = new HostBuilder()
                        .ConfigureLogging()
                        .ConfigureFlightPriceDetection(Headless)
                        .ConfigureServices((_, services) => services.AddHostedService(s => new OneShotScraper(
                            origin,
                            destination,
                            date,
                            s.GetService<IFlightPriceDetections>(),
                            s.GetService<IHostApplicationLifetime>())));
                });
            if (!builder.Parse(args))
            {
                throw new Exception("Could not parse command line arguments!");
            }

            return result!;
        }
    }
}
