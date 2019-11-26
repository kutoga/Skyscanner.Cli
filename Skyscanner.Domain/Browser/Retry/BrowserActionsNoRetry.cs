namespace Skyscanner.Domain.Browser.Retry
{
    using System;
    using System.Threading.Tasks;

    public class BrowserActionsNoRetry : IBrowserActionsRetry
    {
        public async Task<TResult> ExecuteWithRetry<TResult>(IBrowserFactory browserFactory, Func<IBrowser, Task<TResult>> actions)
        {
            using (var browser = browserFactory.GetInstance())
            {
                return await actions(browser).ConfigureAwait(false);
            }
        }
    }
}
