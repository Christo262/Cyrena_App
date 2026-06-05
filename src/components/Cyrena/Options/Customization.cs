namespace Cyrena.Options
{
    public class Customization
    {
        public Customization()
        {
            Background = new Background();
        }

        public const string Key = "customization";
        public string? Mascot { get; set; } = "cyrena_default.png";

        public Background Background { get; set; }

        public string ViewStart { get; set; } = "cyrena.default";
    }

    public class Background
    {
        public string? BackgroundColor { get; set; } = "#212529";
        public float BackgroundOpacity { get; set; } = 0.75f;
        public string? BackgroundImage { get; set; } = "_content/Cyrena/wallpapers/default.png";
    }
}
