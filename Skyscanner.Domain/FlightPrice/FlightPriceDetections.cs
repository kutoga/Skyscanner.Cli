namespace Skyscanner.Domain.FlightPrice
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using Serilog;
    using Serilog.Context;
    using Skyscanner.Model;

    public sealed class FlightPriceDetections : IFlightPriceDetections
    {
        private readonly IDictionary<string, IFlightPriceDetection> flightPriceDetectors;
        private readonly ILogger logger;

        public FlightPriceDetections(IReadOnlyDictionary<string, IFlightPriceDetection> flightPriceDetectors, ILogger logger)
        {
            this.flightPriceDetectors = flightPriceDetectors.ToDictionary(r => r.Key, r => r.Value);
            this.logger = logger.ForContext<FlightPriceDetections>();
        }

        public FlightPriceDetections(ILogger logger, params (string name, IFlightPriceDetection detector)[] detectors)
            : this(detectors.ToImmutableDictionary(d => d.name, d => d.detector), logger)
        {
        }

        public FlightPriceDetections(ILogger logger)
            : this(ImmutableDictionary<string, IFlightPriceDetection>.Empty, logger)
        {
        }

        public void Add(string name, IFlightPriceDetection flightPriceDetection)
        {
            logger.Debug("Registering flightpricedetection with {name}", name);
            flightPriceDetectors.Add(name, flightPriceDetection);
        }

        public async Task<IReadOnlyDictionary<string, double>> DetectFlightPrices(Airport origin, Airport destination, DateTimeOffset date)
        {
            using (LogContext.PushProperty("OriginAirport", origin))
            using (LogContext.PushProperty("DestinationAirport", destination))
            using (LogContext.PushProperty("Date", date))
            {
                var pricesTasks = flightPriceDetectors.Select(async d =>
                {
                    using (LogContext.PushProperty("FlightPriceDetection", d.Key))
                    {
                        return (name: d.Key, price: await d.Value.GetCheapestFlightPriceInEUR(origin, destination, date).ConfigureAwait(false));
                    }
                });
                var prices = await Task.WhenAll(pricesTasks.ToArray()).ConfigureAwait(false);
                var pricesByDetection = prices.ToImmutableDictionary(p => p.name, p => p.price);
                logger.Debug("Detected flight prices: {pricesByDetection}", pricesByDetection);
                return pricesByDetection;
            }
        }
    }
}
