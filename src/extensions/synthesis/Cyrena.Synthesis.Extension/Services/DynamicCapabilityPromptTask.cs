using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Synthesis.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Synthesis.Services
{
    internal class DynamicCapabilityPromptTask : IStartupTask
    {
        private readonly IPromptManager _prompts;
        private readonly ICapabilityStore _store;
        private readonly IIterationService _its;
        public DynamicCapabilityPromptTask(IPromptManager prompts, ICapabilityStore store, IIterationService its)
        {
            _prompts = prompts;
            _store = store;
            _its = its;
        }

        public int Order => 10;

        public Task RunAsync(CancellationToken cancellationToken = default)
        {
            _its.OnIterationStart(async e =>
            {
                await AppendPrompt(cancellationToken);
            });
            return Task.CompletedTask;
        }

        private string? _promptId;
        private async Task AppendPrompt(CancellationToken cancellationToken)
        {
            var prompt = Resources.Read(typeof(DynamicCapabilityPlugin).Assembly, "Cyrena.Synthesis.Resources.instructions.md");
            var sb = new StringBuilder();
            sb.AppendLine(prompt);

            var caps = await _store.GetAllAsync(cancellationToken);
            var count = caps.Where(x => x.IsEnabled).Count();
            if(count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("## Available Dynamic Capabilities");
                sb.AppendLine();
                sb.AppendLine("This is a list of created dynamic capabilities you may use if necessary:");

                foreach (var capability in caps.Where(x => x.IsEnabled))
                {
                    sb.AppendLine($"- {capability.Id}: {capability.Title}");
                }
            }
            if(string.IsNullOrEmpty(_promptId))
                _promptId = _prompts.AddPrompt(5, sb.ToString());
            else
                _prompts.UpdatePrompt(_promptId, sb.ToString());
        }
    }
}
