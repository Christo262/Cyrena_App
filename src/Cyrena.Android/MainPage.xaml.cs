using Cyrena.Options;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.FileProviders;

namespace Cyrena.Android
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
    }

    public class CustomWebView : BlazorWebView
    {
        public override IFileProvider CreateFileProvider(string contentRootDir)
        {
            if (!Directory.Exists(CyrenaBuilder.UserContentDirectory))
                Directory.CreateDirectory(CyrenaBuilder.UserContentDirectory);
            if (!Directory.Exists(CyrenaBuilder.ConversationsData))
                Directory.CreateDirectory(CyrenaBuilder.ConversationsData);
            var fpu = new PhysicalFileProvider(CyrenaBuilder.UserContentDirectory);
            var fpc = new PhysicalFileProvider(CyrenaBuilder.ConversationsData);
            var fpd = base.CreateFileProvider(contentRootDir);
            return new CompositeFileProvider(fpd, fpc, fpu);
        }
    }
}
