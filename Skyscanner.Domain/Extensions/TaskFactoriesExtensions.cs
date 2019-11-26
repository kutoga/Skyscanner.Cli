namespace Skyscanner.Domain.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public static class TaskFactoriesExtensions
    {
        public static async Task SerialExecutionInRandomOrder(this IEnumerable<Func<Task>> taskFactories, Random? random = null)
        {
            var shuffledTaskFactories = (random ?? new Random()).Shuffle(taskFactories);
            foreach (var taskFactory in shuffledTaskFactories)
            {
                await taskFactory().ConfigureAwait(false);
            }
        }
    }
}
