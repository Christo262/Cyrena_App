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
           return  kernel.Services.GetRequiredService<IChatConfigurationService>().Config.Title ?? "New Chat";
        }

        public static string GetId(this Kernel kernel)
        {
            return kernel.Services.GetRequiredService<IChatConfigurationService>().Config.Id;
        }

        public static ChatConfiguration GetConfiguration(this Kernel kernel)
        {
            return kernel.Services.GetRequiredService<IChatConfigurationService>().Config;
        }
    }
}
