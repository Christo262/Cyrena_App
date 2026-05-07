using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Synthesis.Components.Shared;
using Cyrena.Synthesis.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.Synthesis.Services
{
    internal class DynamicCapabilityRequestFunctions
    {
        private readonly IDisplayService _display;
        public DynamicCapabilityRequestFunctions(IDisplayService display)
        {
            _display = display;
        }

        [KernelFunction("request")]
        [Description("Submits a request to the Dynamic Capability Builder to create a new reusable capability. The requested capability may become available after the user reviews and completes the request workflow.")]
        public async Task<ToolResult> RequestCapability(
            [Description("A short, human-friendly title describing the requested capability.")]
            string title,

            [Description("Detailed functional requirements describing what the capability should do, expected behavior, inputs, outputs, and constraints.")]
            string requirements)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(requirements))
                return new ToolResult(false, "title and instruction are both required");

            var model = new ModelCapabilityRequest(title, requirements);
            var result = await _display.ShowModal<Request>(new BootstrapBlazor.Components.ResultDialogOption()
            {
                Size = BootstrapBlazor.Components.Size.Medium,
                Title = "New Capability Request",
                ButtonYesText = "Submit",
                ButtonNoText = "Cancel",
                ComponentParameters = new()
                {
                    {"Model", model }
                }
            });
            if (result == BootstrapBlazor.Components.DialogResult.Yes)
                return new ToolResult(true, "Request Submitted.");
            return new ToolResult(false, "User cancelled request");
        }
    }
}
