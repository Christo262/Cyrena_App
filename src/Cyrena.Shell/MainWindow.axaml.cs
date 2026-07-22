using Avalonia.Controls;
using Cyrena.Extensions;
using Cyrena.Options;

namespace Cyrena.Shell;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var options = CyrenaRuntime.CreateSettings().Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
        Web.Source = new($"http://localhost:{options.ServerPort}");
    }

    public MainWindow(string url, int width, int height)
    {
        InitializeComponent();
        this.Width = width;
        this.Height = height;
        Web.Source = new(url);
    }
}