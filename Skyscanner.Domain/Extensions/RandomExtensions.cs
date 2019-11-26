namespace Skyscanner.Domain.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class RandomExtensions
    {
        public static double NextDouble(this Random random, double minValue, double maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentException($"{nameof(minValue)} must not be larger than {nameof(maxValue)}!");
            }

            return ((random ?? throw new ArgumentNullException(nameof(random))).NextDouble() * (maxValue - minValue)) + minValue;
        }

        public static IEnumerable<T> Shuffle<T>(this Random random, IEnumerable<T> source)
        {
            return source.OrderBy(x => random.Next());
        }
    }
}
