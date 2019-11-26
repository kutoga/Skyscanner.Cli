namespace Skyscanner.Cli
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Skyscanner.Domain.FlightPrice;
    using Skyscanner.Model;

    public class OneShotScraper : BackgroundService
    {
        private readonly Airport origin;
        private readonly Airport destination;
        private readonly DateTimeOffset date;
        private readonly IFlightPriceDetections flightPriceDetections;
        private readonly IHostApplicationLifetime applicationLifetime;

        public OneShotScraper(
            Airport origin,
            Airport destination,
            DateTimeOffset date,
            IFlightPriceDetections flightPriceDetections,
            IHostApplicationLifetime applicationLifetime)
        {
            this.origin = origin;
            this.destination = destination;
            this.date = date;
            this.flightPriceDetections = flightPriceDetections;
            this.applicationLifetime = applicationLifetime;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var prices = await flightPriceDetections.DetectFlightPrices(origin, destination, date)
                .ConfigureAwait(false);
            foreach (var price in prices.OrderBy(p => p.Key))
            {
                await Console.Out.WriteLineAsync($"{price.Key} => {price.Value} EUR").ConfigureAwait(false);
            }

            applicationLifetime.StopApplication();
        }
    }
}
