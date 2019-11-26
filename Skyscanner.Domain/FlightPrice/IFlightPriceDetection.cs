namespace Skyscanner.Domain.FlightPrice
{
    using System;
    using System.Threading.Tasks;
    using Skyscanner.Model;

    public interface IFlightPriceDetection
    {
        Task<double> GetCheapestFlightPriceInEUR(Airport origin, Airport destination, DateTimeOffset date);
    }
}
