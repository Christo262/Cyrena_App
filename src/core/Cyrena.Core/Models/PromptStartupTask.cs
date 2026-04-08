using Cyrena.Contracts;
using Cyrena.Extensions;

namespace Cyrena.Models
{
    /// <summary>
    /// Easier way to add system prompts
    /// </summary>
    internal class PromptStartupTask : IStartupTask
    {
        private readonly IChatMessageService _chat;
        private readonly string _prompt;
        public PromptStartupTask(IChatMessageService chat, string prompt)
        {
            _chat = chat;
            _prompt = prompt;
        }

        public int Order => 1;

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            await _chat.AddSystemMessage(_prompt);
        }
    }
}
