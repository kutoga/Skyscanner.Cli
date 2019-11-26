namespace Skyscanner.Domain.Browser.Delayed
{
    using System;
    using System.Threading.Tasks;

    public sealed class DelayedBrowser : IBrowser
    {
        private readonly IBrowser wrappedBrowser;

        public DelayedBrowser(IBrowser wrappedBrowser)
        {
            this.wrappedBrowser = wrappedBrowser;
            AfterNavigationDelayOptions = DelayOptions.DefaultNavigationDelayOptions;
            BeforeClickDelayOptions = DelayOptions.DefaultBeforeClickDelayOptions;
            BeforeMouseMoveDelayOptions = DelayOptions.DefaultNavigationDelayOptions;
            TypingDelay = DelayOptions.DefaultTypingDelayOptions;
        }

        public IDelayOptions AfterNavigationDelayOptions { get; set; }

        public IDelayOptions BeforeClickDelayOptions { get; set; }

        public IDelayOptions BeforeMouseMoveDelayOptions { get; set; }

        public IDelayOptions TypingDelay { get; set; }

        public void Dispose()
        {
            wrappedBrowser.Dispose();
        }

        public async Task<IBrowserElement> GetElementByClass(string elementClass)
        {
            return new DelayedBrowserElement(await wrappedBrowser.GetElementByClass(elementClass).ConfigureAwait(false), this);
        }

        public async Task<IBrowserElement> GetElementById(string elementId)
        {
            return new DelayedBrowserElement(await wrappedBrowser.GetElementById(elementId).ConfigureAwait(false), this);
        }

        public async Task NavigateTo(Uri url)
        {
            await wrappedBrowser.NavigateTo(url).ConfigureAwait(false);
            await Task.Delay(AfterNavigationDelayOptions.GetNextDelay()).ConfigureAwait(false);
        }

        public Task<object> ExecuteScript(string script, params object[] parameters)
        {
            return wrappedBrowser.ExecuteScript(script, parameters);
        }

        public async Task SendUpArrowKey()
        {
            await Task.Delay(TypingDelay.GetNextDelay()).ConfigureAwait(false);
            await wrappedBrowser.SendUpArrowKey().ConfigureAwait(false);
        }

        public async Task SendDownArrowKey()
        {
            await Task.Delay(TypingDelay.GetNextDelay()).ConfigureAwait(false);
            await wrappedBrowser.SendDownArrowKey().ConfigureAwait(false);
        }

        public string GetPageSource()
        {
            return wrappedBrowser.GetPageSource();
        }
    }
}
