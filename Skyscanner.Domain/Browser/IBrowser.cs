namespace Skyscanner.Domain.Browser
{
    using System;
    using System.Threading.Tasks;

    public interface IBrowser : IDisposable
    {
        Task NavigateTo(Uri url);

        Task<IBrowserElement> GetElementById(string elementId);

        Task<IBrowserElement> GetElementByClass(string elementClass);

        Task<object> ExecuteScript(string script, params object[] parameters);

        Task SendUpArrowKey();

        Task SendDownArrowKey();

        string GetPageSource();
    }
}
