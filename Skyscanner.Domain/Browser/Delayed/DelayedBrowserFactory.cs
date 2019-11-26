namespace Skyscanner.Domain.Browser.Delayed
{
    using System;
    using Serilog;

    public sealed class DelayedBrowserFactory : IBrowserFactory
    {
        private readonly IBrowserFactory wrappedFactory;
        private readonly ILogger logger;

        public DelayedBrowserFactory(IBrowserFactory wrappedFactory, ILogger logger)
        {
            this.wrappedFactory = wrappedFactory;
            this.logger = logger.ForContext<DelayedBrowserFactory>();
        }

        public IBrowser GetInstance()
        {
            logger.Debug("Create instance.");
            return new DelayedBrowser(wrappedFactory.GetInstance());
        }
    }
}
