using Cyrena.Contracts;
using Cyrena.Desktop.Models;
using Cyrena.Models;
using Photino.Blazor;
using Photino.NET;

namespace Cyrena.Desktop.Services
{
    internal class BrowserService : IBrowserService
    {
        private readonly PhotinoBlazorApp _app;
        private readonly List<IWindowHandle> _handles;
        public BrowserService(PhotinoBlazorApp app)
        {
            _app = app;
            _handles = new List<IWindowHandle>();
        }

        public void Dispose()
        {
            foreach (var handle in _handles)
                handle.Dispose();
        }

        public IWindowHandle OpenUri(Uri uri, string title = "Cyréna")
        {
            var window = new PhotinoWindow(_app.MainWindow);
            window.SetIconFile("favicon.ico")
                .SetTitle(title)
                .Load(uri)
                .SetTransparent(false)
                .SetDevToolsEnabled(true)
                .SetContextMenuEnabled(true)
                .SetFileSystemAccessEnabled(true)
                .Center();
            var handle = new WindowHandle(window);
            handle.Closing += Handle_Closing;
            _handles.Add(handle);
            window.WaitForClose();
            return handle;
        }

        public IWindowHandle OpenFile(string filePath, string title = "Cyréna")
        {
            var window = new PhotinoWindow(_app.MainWindow);
            window.SetIconFile("favicon.ico")
                .SetTitle(title)
                .Load(filePath)
                .SetTransparent(false)
                .SetDevToolsEnabled(true)
                .SetFileSystemAccessEnabled(true)
                .SetContextMenuEnabled(true)
                .Center();
            var handle = new WindowHandle(window);
            handle.Closing += Handle_Closing;
            _handles.Add(handle);
            window.WaitForClose();
            return handle;
        }

        public IWindowHandle OpenRawHtml(string html, string title = "Cyréna")
        {
            var window = new PhotinoWindow(_app.MainWindow);
            window.SetIconFile("favicon.ico")
                .SetTitle(title)
                .LoadRawString(html)
                .SetFileSystemAccessEnabled(true)
                .SetTransparent(false)
                .SetDevToolsEnabled(true)
                .SetContextMenuEnabled(true)
                .Center();
            var handle = new WindowHandle(window);
            handle.Closing += Handle_Closing;
            _handles.Add(handle);
            window.WaitForClose();
            return handle;
        }

        private void Handle_Closing(object? sender, EventArgs e)
        {
            var handle = (WindowHandle)sender!;
            handle.Closing -= Handle_Closing;
            _handles.Remove(handle);
        }
    }
}
