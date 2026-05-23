namespace Cyrena.Options
{
    public class ApplicationOptions
    {
        public const string Key = "cyrena.application";

        public double Width { get; set; } = 800;
        public double Height { get; set; } = 450;

        public bool LaunchWindowOnStartup { get; set; } = true;
        public int ServerPort { get; set; } = 8000;
        public string? DefaultConnectionId { get; set; }
    }
}
