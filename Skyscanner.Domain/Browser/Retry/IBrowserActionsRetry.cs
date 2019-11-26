namespace Skyscanner.Domain.Browser.Retry
{
    using System;
    using System.Threading.Tasks;

    public interface IBrowserActionsRetry
    {
        Task<TResult> ExecuteWithRetry<TResult>(IBrowserFactory browserFactory, Func<IBrowser, Task<TResult>> actions);
    }
}
