using Photino.NET;
using System.Runtime.InteropServices;
using System.Text.Json;

internal sealed record AppArgs(
    string Title,
    string Url,
    int Width,
    int Height)
{
    public static AppArgs Parse(string[] args)
    {
        var result = new AppArgs(
            Title: "Cyréna",
            Url: "http://localhost:8000",
            Width: 800,
            Height: 600);

        for (var i = 0; i < args.Length; i++)
        {
            var next = i + 1 < args.Length ? args[i + 1] : null;

            switch (args[i])
            {
                case "--title" when next is not null:
                    result = result with { Title = next };
                    i++;
                    break;

                case "--url" when next is not null:
                    result = result with { Url = next };
                    i++;
                    break;

                case "--width" when next is not null && int.TryParse(next, out var width):
                    result = result with { Width = width };
                    i++;
                    break;

                case "--height" when next is not null && int.TryParse(next, out var height):
                    result = result with { Height = height };
                    i++;
                    break;
            }
        }

        return result;
    }
}

internal static class Program
{
    private static string GetIconPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "favicon.png";

        return "favicon.ico";
    }

    [STAThread]
    private static void Main(string[] args)
    {
        var appArgs = AppArgs.Parse(args);
        var window = new PhotinoWindow()
            .SetTitle(appArgs.Title)
            .SetIconFile(GetIconPath())
            .SetUseOsDefaultLocation(false)
            .SetUseOsDefaultSize(false)
            .SetWidth(appArgs.Width)
            .SetHeight(appArgs.Height)
            .SetContextMenuEnabled(true)
            .SetDevToolsEnabled(false)
            .SetFileSystemAccessEnabled(true)
            .Load(new Uri(appArgs.Url));

        window.WindowSizeChanged += Window_WindowSizeChanged;

        window.WaitForClose();
    }

    private static int _renderCount { get; set; }
    private static void Window_WindowSizeChanged(object? sender, System.Drawing.Size e)
    {
        try
        {
            if (_renderCount < 5) //Shim: some distros hit this a number of times just on opening without user actually resizing
            {
                _renderCount++;
                return;
            }
            var obj = new
            {
                width = e.Width,
                height = e.Height
            };
            var json = JsonSerializer.Serialize(obj);
            File.WriteAllText("./photino.json", json);
        }
        catch { }
    }
}
