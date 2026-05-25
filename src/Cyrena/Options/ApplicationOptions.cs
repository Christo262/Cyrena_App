using System.ComponentModel.DataAnnotations;

namespace Cyrena.Options
{
    public class ApplicationOptions
    {
        public const string Key = "cyrena.application";

        public int Width { get; set; } = 800;
        public int Height { get; set; } = 450;

        public bool LaunchWindowOnStartup { get; set; } = true;
        public int ServerPort { get; set; } = 8000;
        public string? DefaultConnectionId { get; set; }

        public bool DarkMode { get; set; } = true;

        public bool UsePin { get; set; }
        [MinLength(4)]
        [MaxLength(6)]
        public string? Pin { get; set; }
    }
}
