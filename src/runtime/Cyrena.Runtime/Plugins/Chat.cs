using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.Runtime.Plugins
{
    internal class Chat
    {
        private readonly IChatConfigurationService _config;
        public Chat(IChatConfigurationService config)
        {
            _config = config;
        }

        [KernelFunction("set_title")]
        [Description("Use this to set the title of a chat to simplify navigation for User.")]
        public async Task<ToolResult> SetTitle(string title)
        {
            _config.Config.Title = title;
            await _config.SaveConfigurationAsync();
            return new ToolResult(true, "Title updated");
        }
    }
}
