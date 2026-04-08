using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Extensions
{
    public static class KernelBuilderExtensions
    {
        public static void AddStartupTask<TStartupTask>(this IKernelBuilder builder)
            where TStartupTask: class, IStartupTask
        {
            builder.Services.AddSingleton<IStartupTask, TStartupTask>();
        }

        public static void AddSystemPrompt(this IKernelBuilder builder, string prompt)
        {
            builder.Services.AddSingleton<IStartupTask>(sp =>
            {
                var chat = sp.GetRequiredService<IChatMessageService>();
                return new PromptStartupTask(chat, prompt);
            });
        }
    }
}
