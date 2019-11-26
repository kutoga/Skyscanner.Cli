namespace Skyscanner.Domain.Browser.Delayed
{
    using System;
    using Skyscanner.Domain.Extensions;

    public sealed class DelayOptions : IDelayOptions
    {
        private readonly Random? random;

        public DelayOptions(TimeSpan minDelay = default, TimeSpan maxDelay = default)
        {
            if (minDelay > maxDelay)
            {
                throw new ArgumentException($"{nameof(minDelay)} must not be larger than {nameof(maxDelay)}!");
            }

            if (minDelay < TimeSpan.Zero)
            {
                throw new ArgumentException($"{nameof(minDelay)} must be 0 or larger!");
            }

            MinDelay = minDelay;
            MaxDelay = maxDelay;

            if (minDelay != maxDelay)
            {
                random = new Random();
            }
        }

        public static DelayOptions DefaultBeforeClickDelayOptions { get; } = new DelayOptions(
            TimeSpan.FromMilliseconds(50),
            TimeSpan.FromMilliseconds(500));

        public static DelayOptions DefaultNavigationDelayOptions { get; } = new DelayOptions(
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromMilliseconds(5000));

        public static DelayOptions DefaultTypingDelayOptions { get; } = new DelayOptions(
            TimeSpan.FromMilliseconds(10),
            TimeSpan.FromMilliseconds(200));

        public TimeSpan MinDelay { get; }

        public TimeSpan MaxDelay { get; }

        public TimeSpan GetNextDelay()
        {
            if (random is null)
            {
                return MinDelay;
            }

            return TimeSpan.FromMilliseconds(random.NextDouble(MinDelay.TotalMilliseconds, MaxDelay.TotalMilliseconds));
        }
    }
}
