namespace Cyrena.Mobile
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            var currentTheme = Application.Current?.RequestedTheme;
            if (currentTheme == AppTheme.Dark)
                IndexFile = "wwwroot/dark.html";
            else
                IndexFile = "wwwroot/light.html";
            blazorWebView.HostPage = IndexFile;
        }

        public string IndexFile { get; set; } = "wwwroot/light.html";
    }
}
