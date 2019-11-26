namespace Skyscanner.Domain.FlightPrice
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Skyscanner.Model;

    public interface IFlightPriceDetections
    {
        Task<IReadOnlyDictionary<string, double>> DetectFlightPrices(Airport origin, Airport destination, DateTimeOffset date);
    }
}
