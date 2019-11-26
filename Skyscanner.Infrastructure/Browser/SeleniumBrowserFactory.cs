namespace Skyscanner.Infrastructure.Browser
{
    using System;
    using OpenQA.Selenium.Firefox;
    using OpenQA.Selenium.Remote;
    using Serilog;
    using Skyscanner.Domain.Browser;

    public sealed class SeleniumBrowserFactory : IBrowserFactory
    {
        private readonly Func<RemoteWebDriver> builder;
        private readonly ILogger logger;

        public SeleniumBrowserFactory(Func<RemoteWebDriver> builder, ILogger logger)
        {
            this.builder = builder;
            this.logger = logger.ForContext<SeleniumBrowserFactory>();
        }

        public static SeleniumBrowserFactory GetFirefoxBrowserFactory(ILogger logger, bool headless = true, string geckoDriverPath = ".")
        {
            var options = new FirefoxOptions();
            options.AddArguments("-private", "--window-size=1920,1080");
            if (headless)
            {
                options.AddArgument("-headless");
            }

            return new SeleniumBrowserFactory(() => new FirefoxDriver(geckoDriverPath, options), logger);
        }

        public IBrowser GetInstance()
        {
            logger.Debug("Get a new browser instance.");
            return new SeleniumBrowser(builder(), logger);
        }
    }
}
