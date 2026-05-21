using System.Windows;

namespace Cyrena.HUD.Windows
{
    /// <summary>
    /// Interaction logic for WebViewCompat.xaml
    /// </summary>
    public partial class WebViewCompat : Window
    {
        public WebViewCompat()
        {
            InitializeComponent();            
        }

        public WebViewCompat(Uri uri, string title = "Cyréna")
        {
            InitializeComponent();
            _web.Source = uri;
            Title = title;
        }

        private readonly HtmlOpener? _html;

        public WebViewCompat(HtmlOpener html, string title = "Cyréna")
        {
            _html = html;
            Title = title;

            InitializeComponent();

            Loaded += Window_Loaded;
        }

        private readonly FileOpener? _file;
        public WebViewCompat(FileOpener file, string title = "Cyréna")
        {
            _file = file;
            Title = title;

            InitializeComponent();

            Loaded += Window_Loaded;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Window_Loaded;

            if(_html != null)
            {
                await _web.EnsureCoreWebView2Async();
                _web.NavigateToString(_html.Content);
            }  
            
            if(_file != null)
            {
                await _web.EnsureCoreWebView2Async();
                _web.Source = new Uri(_file.FilePath);
            }
        }
    }

    public record FileOpener(string FilePath);
    public record HtmlOpener(string Content);
}
