using Cyrena.Contracts;

namespace Cyrena.ClassLibrary.Extensions
{
    public static class DeveloperContextExtensions
    {
        public static bool IsTrackedFile(this IDeveloperContext ctx, string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();

            return ext is ".cs"
                or ".json"
                or ".md";
        }
    }
}
