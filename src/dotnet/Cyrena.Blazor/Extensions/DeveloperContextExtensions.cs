using Cyrena.Contracts;

namespace Cyrena.Blazor.Extensions
{
    public static class DeveloperContextExtensions
    {
        public static bool IsTrackedFile(this IDeveloperContext ctx, string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();

            return ext is ".cs"
                or ".razor"
                or ".json"
                or ".css"
                or ".md"
                or ".js";
        }
    }
}
