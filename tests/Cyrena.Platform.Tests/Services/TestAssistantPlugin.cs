using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Text;

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
            builder.Plugins.AddFromType<DisplayServiceTests>("Modal");
            return Task.CompletedTask;
        }
    }
}
