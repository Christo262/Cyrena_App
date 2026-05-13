namespace Cyrena.Extensions
{
    public static class ComponentExtensions
    {
        public static (int r, int g, int b) HexToRgb(string hex)
        {
            hex = hex.TrimStart('#');

            if (hex.Length == 3) // Short form like "#F80"
                hex = string.Concat(hex.Select(c => new string(c, 2)));

            return (
                r: Convert.ToInt32(hex.Substring(0, 2), 16),
                g: Convert.ToInt32(hex.Substring(2, 2), 16),
                b: Convert.ToInt32(hex.Substring(4, 2), 16)
            );
        }

        public static string ToCssRgbFromHex(this string hex)
        {
            (int r, int g, int b) = HexToRgb(hex);
            return $"{r}, {g}, {b}";
        }

        public static string ToCssFloat(this float value)
        {
            return value.ToString().Replace(",", ".");
        }
    }
}
