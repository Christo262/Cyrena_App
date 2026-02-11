using Cyrena.Contracts;

namespace Cyrena.PlatformIO.Extensions
{
    public static class DeveloperContextExtensions
    {
        public static bool IsTrackedFile(this IDeveloperContext ctx, string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();

            return ext is ".ini"
                or ".json"
                or ".c"
                or ".cpp"
                or ".h";
        }
    }
}
