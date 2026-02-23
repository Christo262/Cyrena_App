namespace Cyrena.HUD.Options
{
    internal class WindowOptions
    {
        public const string Key = "cyrena.hud";

        public bool Ctrl { get; set; } = true;
        public bool Alt { get; set; }
        public bool Shift { get; set; } = true;
        public bool Win { get; set; }

        public uint VirtualKey { get; set; } = 0x51;

        public string Display =>
            $"{(Ctrl ? "Ctrl+" : "")}" +
            $"{(Alt ? "Alt+" : "")}" +
            $"{(Shift ? "Shift+" : "")}" +
            $"{(Win ? "Win+" : "")}" +
            $"{KeyName}";

        public string KeyName { get; set; } = "Q";

        public string? DefaultConnectionId { get; set; }
    }
}
