namespace Skyscanner.Infrastructure.Browser.Retry
{
    using System;
    using System.Threading.Tasks;
    using Polly;
    using Serilog;
    using Skyscanner.Domain.Browser;
    using Skyscanner.Domain.Browser.Retry;

    public class ThreeTimesBrowserActionsRetry : IBrowserActionsRetry
    {
        private readonly ILogger logger;

        public ThreeTimesBrowserActionsRetry(ILogger logger)
        {
            this.logger = logger.ForContext<ThreeTimesBrowserActionsRetry>();
        }

        public Task<TResult> ExecuteWithRetry<TResult>(IBrowserFactory browserFactory, Func<IBrowser, Task<TResult>> actions)
        {
            return Policy
                .Handle<Exception>()
                .RetryAsync(3)
                .ExecuteAsync(async () =>
                {
                    try
                    {
                        using (var browser = browserFactory.GetInstance())
                        {
                            return await actions(browser).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Browser actions failed.");
                        throw;
                    }
                });
        }
    }
}
