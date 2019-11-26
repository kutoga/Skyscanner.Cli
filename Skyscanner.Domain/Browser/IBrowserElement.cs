namespace Skyscanner.Domain.Browser
{
    using System.Threading.Tasks;

    public interface IBrowserElement
    {
        Task Click();

        Task MoveMouseToElement();

        Task SendKeys(string keys);

        Task SendEnter();

        Task SetInnerText(string innerText);
    }
}
