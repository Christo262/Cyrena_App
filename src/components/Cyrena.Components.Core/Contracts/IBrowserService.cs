using Cyrena.Models;

namespace Cyrena.Contracts
{
    /// <summary>
    /// Open new browser windows
    /// </summary>
    public interface IBrowserService : IDisposable
    {
        IWindowHandle OpenUri(Uri uri, string title = "Cyréna");
        IWindowHandle OpenFile(string filePath, string title = "Cyréna");
        IWindowHandle OpenRawHtml(string html, string title = "Cyréna");
    }
}
