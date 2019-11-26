namespace Skyscanner.Domain.Browser.Delayed
{
    using System.Globalization;
    using System.Threading.Tasks;

    public sealed class DelayedBrowserElement : IBrowserElement
    {
        private readonly IBrowserElement wrappedElement;
        private readonly DelayedBrowser browser;

        public DelayedBrowserElement(IBrowserElement wrappedElement, DelayedBrowser browser)
        {
            this.wrappedElement = wrappedElement;
            this.browser = browser;
        }

        public async Task Click()
        {
            await MoveMouseToElement().ConfigureAwait(false);
            await Task.Delay(browser.BeforeClickDelayOptions.GetNextDelay()).ConfigureAwait(false);
            await wrappedElement.Click().ConfigureAwait(false);
        }

        public async Task MoveMouseToElement()
        {
            await Task.Delay(browser.BeforeMouseMoveDelayOptions.GetNextDelay()).ConfigureAwait(false);
            await wrappedElement.MoveMouseToElement().ConfigureAwait(false);
        }

        public async Task SendKeys(string keys)
        {
            await MoveMouseToElement().ConfigureAwait(false);
            foreach (var key in keys)
            {
                await Task.Delay(browser.TypingDelay.GetNextDelay()).ConfigureAwait(false);
                await wrappedElement.SendKeys(key.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);
            }
        }

        public async Task SendEnter()
        {
            await Task.Delay(browser.TypingDelay.GetNextDelay()).ConfigureAwait(false);
            await wrappedElement.SendEnter().ConfigureAwait(false);
        }

        public Task SetInnerText(string innerText)
        {
            return wrappedElement.SetInnerText(innerText);
        }
    }
}
