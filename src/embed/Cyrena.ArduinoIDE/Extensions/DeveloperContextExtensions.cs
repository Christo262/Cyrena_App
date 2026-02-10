using Cyrena.Contracts;

namespace Cyrena.ArduinoIDE.Extensions
{
    public static class DeveloperContextExtensions
    {
        public static bool IsTrackedFile(this IDeveloperContext ctx, string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();

            return ext is ".ino"
                or ".cpp"
                or ".h";
        }
    }
}
