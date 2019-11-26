namespace Skyscanner.Domain.FlightPrice
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Skyscanner.Domain.Browser;
    using Skyscanner.Domain.Browser.Retry;
    using Skyscanner.Domain.Extensions;
    using Skyscanner.Model;

    public sealed class SkyscannerFlightPriceDetection : IFlightPriceDetection
    {
        private static readonly Regex PriceExtractionPattern = new Regex(
            @"Billigster Flug.*?(?<price>\d+) €",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly IBrowserFactory browserFactory;
        private readonly IBrowserActionsRetry browserActionsRetry;

        public SkyscannerFlightPriceDetection(IBrowserFactory browserFactory, IBrowserActionsRetry browserActionsRetry)
        {
            this.browserFactory = browserFactory;
            this.browserActionsRetry = browserActionsRetry;
        }

        public Task<double> GetCheapestFlightPriceInEUR(Airport origin, Airport destination, DateTimeOffset date)
        {
            return browserActionsRetry.ExecuteWithRetry(browserFactory, async browser =>
            {
                await browser.NavigateTo(new Uri("https://www.skyscanner.de/")).ConfigureAwait(false);

                var cookieBannerCloseClass = await GetSingleClassWithFirstPrefix(
                    browser,
                    "button",
                    "CookieBanner_CookieBanner__button__",
                    "CookieBanner_CookieBanner__close-button__").ConfigureAwait(false);
                var cookieBannerCloseButton = await browser.GetElementByClass(cookieBannerCloseClass).ConfigureAwait(false);
                await cookieBannerCloseButton.Click().ConfigureAwait(false);

                var oneWay = await browser.GetElementById("fsc-trip-type-selector-one-way").ConfigureAwait(false);
                await oneWay.Click().ConfigureAwait(false);

                await new Func<Task>[]
                {
                    () => WriteAirportNameToField(browser, "fsc-origin-search", origin),
                    () => WriteAirportNameToField(browser, "fsc-destination-search", destination),
                    () => SelectDate(browser, date)
                }.SerialExecutionInRandomOrder().ConfigureAwait(false);

                var searchButton = await browser.GetElementByClass(
                    await GetSingleClassWithPrefix(browser, "button", "App_submit-button__").ConfigureAwait(false)).ConfigureAwait(false);
                await searchButton.Click().ConfigureAwait(false);

                if (!await browser.WaitUntilPageSourceContains("Ergebnisse</span>", TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(60)).ConfigureAwait(false))
                {
                    throw new Exception("Could not retrive results!");
                }

                return (double)ExtractpriceInEUR(browser.GetPageSource());
            });
        }

        private static int ExtractpriceInEUR(string pageSource)
        {
            return int.Parse(PriceExtractionPattern.Match(pageSource).Groups["price"].Value, CultureInfo.InvariantCulture);
        }

        private static async Task WriteAirportNameToField(IBrowser browser, string targetElementId, Airport airport)
        {
            var element = await browser.GetElementById(targetElementId).ConfigureAwait(false);
            await element.SendKeys(" " + GetAirportInputFieldName(airport)).ConfigureAwait(false);
            await element.SendEnter().ConfigureAwait(false);
        }

        private static async Task SelectDate(IBrowser browser, DateTimeOffset date)
        {
            var maxMonthsInFuture = 12;
            var monthsToMove = MonthsBetween(DateTimeOffset.Now, date);
            if (monthsToMove < 0)
            {
                throw new ArgumentException($"Cannot select a date in the past ({date})!");
            }

            if (monthsToMove > maxMonthsInFuture)
            {
                throw new ArgumentException($"Cannot compute flight prices which are more than {maxMonthsInFuture} months in the future ({date})!");
            }

            var dateInput = await GetElementByClassPrefix(browser, "button", "DateInput_DateInput__").ConfigureAwait(false);
            await dateInput.Click().ConfigureAwait(false);

            var monthSelectorClasses = await GetClassesWithPrefixNameScript()(browser, "select", "BpkSelect_bpk-select__").ConfigureAwait(false);
            var monthSelector = await browser.GetElementByClass(monthSelectorClasses.Last()).ConfigureAwait(false);
            foreach (var month in Enumerable.Range(0, monthsToMove))
            {
                await MoveOneMonthDown().ConfigureAwait(false);
            }

            await GetDayOfMonthSelectorScript()(browser, date.Day).ConfigureAwait(false);

            async Task MoveOneMonthDown()
            {
                await monthSelector.SendEnter().ConfigureAwait(false);
                await browser.SendDownArrowKey().ConfigureAwait(false);
                await monthSelector.SendEnter().ConfigureAwait(false);
            }
        }

        private static async Task<IBrowserElement> GetElementByClassPrefix(IBrowser browser, string targetNodeType, string classPrefix)
        {
            var className = await GetSingleClassWithPrefix(browser, targetNodeType, classPrefix).ConfigureAwait(false);
            return await browser.GetElementByClass(className).ConfigureAwait(false);
        }

        private static async Task<string> GetSingleClassWithFirstPrefix(IBrowser browser, string targetNodeType, params string[] classPrefixes)
        {
            foreach (var classPrefix in classPrefixes)
            {
                var classesWithPrefix = await GetClassesWithPrefixNameScript()(browser, targetNodeType, classPrefix).ConfigureAwait(false);
                if (classesWithPrefix.Length == 1)
                {
                    return classesWithPrefix.Single();
                }
            }

            throw new Exception("Could not find any suitable class!");
        }

        private static async Task<string> GetSingleClassWithPrefix(IBrowser browser, string targetNodeType, string classPrefix)
        {
            var classesWithPrefix = await GetClassesWithPrefixNameScript()(browser, targetNodeType, classPrefix).ConfigureAwait(false);
            if (classesWithPrefix.Length != 1)
            {
                throw new Exception($"Expected exactly one class with the prefix '{classPrefix}', but found {classesWithPrefix.Length}!");
            }

            return classesWithPrefix[0];
        }

        private static Func<IBrowser, int, Task> GetDayOfMonthSelectorScript()
        {
            return async (browser, dayOfMonth) =>
            {
                var calendarDateButtonClass = await GetSingleClassWithPrefix(browser, "button", "BpkCalendarDate_bpk-calendar-date__").ConfigureAwait(false);
                var script = string.Concat(new[]
                {
                    JSAssignArgumentToVariable(0, "day_of_month"),
                    JSAssignArgumentToVariable(1, "calendar_date_button_class"),
                    //TODO: simpligy to assign all variables at once

                    "day_buttons = Array.from($('.' + calendar_date_button_class));",
                    "days = day_buttons.map(x => parseInt(x.children[0].innerText));",
                    "first_day_of_month_index = days.indexOf(1);",
                    "day_buttons.splice(0, first_day_of_month_index);",
                    "day_buttons[day_of_month - 1].click();"
                });
                await browser.ExecuteScript(script, dayOfMonth, calendarDateButtonClass).ConfigureAwait(false);
            };
        }

        private static Func<IBrowser, string, string, Task<string[]>> GetClassesWithPrefixNameScript()
        {
            var script = string.Concat(new[]
            {
                JSAssignArgumentToVariable(0, "target_node_type"),
                JSAssignArgumentToVariable(1, "class_prefix"),
                "all_classes = Array.from(new Set(Array.prototype.slice",
                "   .call(document.querySelectorAll(target_node_type))",
                "   .map(b => b.getAttribute('class').split(' ')).reduce((a, b) => a.concat(b))));",
                "valid_classes = all_classes.filter(c => c.startsWith(class_prefix));",
                JSReturn("valid_classes")
            });
            return async (browser, targetNodeType, classPrefix) =>
            {
                var resultingObjects = (IEnumerable<object>)await browser.ExecuteScript(script, targetNodeType, classPrefix).ConfigureAwait(false);
                return resultingObjects.Cast<string>().ToArray();
            };
        }

        private static string JSAssignArgumentToVariable(int argumentIndex, string variableName) =>
            $"{variableName} = arguments[{argumentIndex}];";

        private static string JSReturn(string expression) => $"return {expression};";

        private static string GetAirportInputFieldName(Airport airport)
        {
            return airport.ToString();
        }

        private static int MonthsBetween(DateTimeOffset smallerDate, DateTimeOffset largerDate)
        {
            return ((largerDate.Year - smallerDate.Year) * 12) + (largerDate.Month - smallerDate.Month);
        }
    }
}
