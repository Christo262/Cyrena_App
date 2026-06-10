using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Platform.Tests.Components.Shared;
using Microsoft.SemanticKernel;

namespace Cyrena.Platform.Tests.Services
{
    internal class TestAssistantPlugin : IAssistantPlugin
    {
        public string Id => "cyrena.platform.tests";

        public string[] Modes => [IAssistantMode.AssistantModeDefault];

        public int Priority => 10;

        public bool Required => false;

        public string Title => "Platform Tests";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            return Task.CompletedTask;
        }
    }
}
