namespace Skyscanner.Domain.Browser.Delayed
{
    using System;

    public interface IDelayOptions
    {
        TimeSpan GetNextDelay();
    }
}
