namespace Skyscanner.Infrastructure.Browser
{
    using System.Threading.Tasks;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Interactions;
    using OpenQA.Selenium.Remote;
    using Serilog;
    using Skyscanner.Domain.Browser;

    public sealed class SeleniumBrowserElement : IBrowserElement
    {
        private readonly RemoteWebDriver driver;
        private readonly IWebElement element;
        private readonly ILogger logger;

        public SeleniumBrowserElement(RemoteWebDriver driver, IWebElement element, ILogger logger)
        {
            this.driver = driver;
            this.element = element;
            this.logger = logger.ForContext<SeleniumBrowserElement>();
        }

        public Task Click()
        {
            logger.Debug("Click on the current element.");
            return Task.Run(() => element.Click());
        }

        public Task MoveMouseToElement()
        {
            logger.Debug("Move to the given element.");
            return Task.Run(() =>
            {
                var actions = new Actions(driver);
                actions.MoveToElement(element);
                actions.Perform();
            });
        }

        public Task SendKeys(string keys)
        {
            logger.Debug("Send the following keys to the current element: {keys}", keys);
            return Task.Run(() => element.SendKeys(keys));
        }

        public Task SendEnter()
        {
            logger.Debug("Send <Enter> to the current element.");
            return Task.Run(() => element.SendKeys(Keys.Enter));
        }

        public Task SetInnerText(string innerText)
        {
            logger.Debug("Set the inner text of the current element as: {innerText}", innerText);
            return Task.Run(() => driver.ExecuteScript("arguments[0].innerText = arguments[1];", element, innerText));
        }
    }
}
