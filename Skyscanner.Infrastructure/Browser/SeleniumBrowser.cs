namespace Skyscanner.Infrastructure.Browser
{
    using System;
    using System.Threading.Tasks;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Interactions;
    using OpenQA.Selenium.Remote;
    using Serilog;
    using Skyscanner.Domain.Browser;

    public sealed class SeleniumBrowser : IBrowser
    {
        private readonly RemoteWebDriver driver;
        private readonly ILogger logger;

        public SeleniumBrowser(RemoteWebDriver driver, ILogger logger)
        {
            this.driver = driver;
            this.logger = logger.ForContext<SeleniumBrowser>();
        }

        public void Dispose()
        {
            driver.Dispose();
        }

        public Task<IBrowserElement> GetElementByClass(string elementClass)
        {
            logger.Debug("Try to get the (if only one exists) element with the class {elementClass}.", elementClass);
            return GetElementBySelector(() => driver.FindElementByClassName(elementClass));
        }

        public Task<IBrowserElement> GetElementById(string elementId)
        {
            logger.Debug("Try to get the element with the id {elementId}.", elementId);
            return GetElementBySelector(() => driver.FindElementById(elementId));
        }

        public Task NavigateTo(Uri url)
        {
            logger.Debug("Navigate to the url {url}.", url);
            return Task.Run(() => driver.Navigate().GoToUrl(url));
        }

        public string GetPageSource()
        {
            return driver.PageSource;
        }

        public Task<object> ExecuteScript(string script, params object[] parameters)
        {
            return Task.Run(() => driver.ExecuteScript(script, parameters));
        }

        public Task SendUpArrowKey()
        {
            logger.Debug("Send <UpArrow>.");
            return SendKeys(Keys.Up);
        }

        public Task SendDownArrowKey()
        {
            logger.Debug("Send <DownArrow>.");
            return SendKeys(Keys.Down);
        }

        private Task<IBrowserElement> GetElementBySelector(Func<IWebElement> selector)
        {
            return Task.Run<IBrowserElement>(() => new SeleniumBrowserElement(driver, selector(), logger.ForContext<SeleniumBrowserElement>()));
        }

        private Task SendKeys(string keys)
        {
            return Task.Run(() =>
            {
                var actions = new Actions(driver);
                actions.SendKeys(keys);
                actions.Perform();
            });
        }
    }
}
