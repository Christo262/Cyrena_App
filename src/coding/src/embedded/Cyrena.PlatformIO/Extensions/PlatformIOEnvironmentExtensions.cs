using Cyrena.PlatformIO.Models;

namespace Cyrena.PlatformIO.Extensions
{
    public static class PlatformIOEnvironmentExtensions
    {
        public static bool IsEspIdf(this PlatformIOEnvironment env)
        {
            return env.Framework != null && env.Framework.Contains("espidf");
        }
    }
}
