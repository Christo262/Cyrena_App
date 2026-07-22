using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Core.View;

namespace Cyrena.Android
{
    [Activity(Theme = "@style/Maui.SplashTheme", 
        MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize 
        | ConfigChanges.Orientation 
        | ConfigChanges.UiMode 
        | ConfigChanges.ScreenLayout 
        | ConfigChanges.SmallestScreenSize 
        | ConfigChanges.Density, WindowSoftInputMode = SoftInput.AdjustResize)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if(Window != null)
            ViewCompat.SetOnApplyWindowInsetsListener(Window.DecorView, new WindowInsetsListener());
        }

        private class WindowInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
        {
            public WindowInsetsCompat OnApplyWindowInsets(global::Android.Views.View? v, global::AndroidX.Core.View.WindowInsetsCompat? insets)
            {
                if (v == null || insets == null) return new WindowInsetsCompat(null);

                var imeHeight = insets.GetInsets(WindowInsetsCompat.Type.Ime())!.Bottom;
                var navHeight = insets.GetInsets(WindowInsetsCompat.Type.NavigationBars())!.Bottom;

                v.SetPadding(0, 0, 0, Math.Max(0, imeHeight - navHeight));
                return ViewCompat.OnApplyWindowInsets(v, insets) ?? insets;
            }
        }
    }
}
