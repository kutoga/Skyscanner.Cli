namespace Skyscanner.Domain.Browser
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public static class IBrowserExtensions
    {
        public static async Task<bool> WaitUntilPageSourceMatches(this IBrowser browser, Func<string, bool> predicate, TimeSpan checkEvery, TimeSpan timeout)
        {
            var stopWatch = Stopwatch.StartNew();
            while (!predicate(browser.GetPageSource()))
            {
                if (timeout > TimeSpan.Zero && stopWatch.Elapsed >= timeout)
                {
                    return false;
                }

                await Task.Delay(checkEvery).ConfigureAwait(false);
            }

            return true;
        }

        public static Task<bool> WaitUntilPageSourceContains(this IBrowser browser, string text, TimeSpan checkEvery, TimeSpan timeout)
        {
            return browser.WaitUntilPageSourceMatches(p => p.Contains(text), checkEvery, timeout);
        }
    }
}
