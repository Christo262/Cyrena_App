using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Extensions
{
    public static class KernelExtensions
    {
        public static string GetTitle(this Kernel kernel)
        {
            try
            {
                return kernel.Services.GetService<IChatConfigurationService>()?.Config.Title ?? "New Chat";
            }
            catch { return string.Empty; }
        }

        public static string GetId(this Kernel kernel)
        {
            try
            {
                return kernel.Services.GetService<IChatConfigurationService>()?.Config.Id ?? string.Empty;
            }catch { return string.Empty; }
        }

        public static ChatConfiguration GetConfiguration(this Kernel kernel)
        {
            return kernel.Services.GetRequiredService<IChatConfigurationService>().Config;
        }
    }
}
