using Cyrena.Contracts;
using Cyrena.HUD.Models;
using Cyrena.HUD.Windows;
using Cyrena.Models;

namespace Cyrena.HUD.Services
{
    internal class BrowserService : IBrowserService
    {
        private readonly List<IWindowHandle> _handles;
        public BrowserService()
        {
            _handles = new List<IWindowHandle>();
        }

        public void Dispose()
        {
            foreach (var handle in _handles) 
                handle.Dispose();
            _handles.Clear();
        }

        public IWindowHandle OpenFile(string filePath, string title = "Cyréna")
        {
            var window = new WebViewCompat(new FileOpener(filePath), title);
            window.Topmost = true;
            var handle = new WindowHandle(window);
            handle.Closing += Handle_Closing;
            window.Show();
            return handle;
        }

        public IWindowHandle OpenRawHtml(string html, string title = "Cyréna")
        {
            var window = new WebViewCompat(new HtmlOpener(html), title);
            window.Topmost = true;
            var handle = new WindowHandle(window);
            handle.Closing += Handle_Closing;
            window.Show();
            return handle;
        }

        public IWindowHandle OpenUri(Uri uri, string title = "Cyréna")
        {
            var window = new WebViewCompat(uri, title);
            window.Topmost = true;
            var handle = new WindowHandle(window);
            handle.Closing += Handle_Closing;
            window.Show();
            return handle;
        }

        private void Handle_Closing(object? sender, EventArgs e)
        {
            var handle = (IWindowHandle)sender!;
            handle.Closing -= Handle_Closing;
            _handles.Remove(handle);
        }
    }
}
