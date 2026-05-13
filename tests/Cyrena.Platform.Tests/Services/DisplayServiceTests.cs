using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.Platform.Tests.Services
{
    internal class DisplayServiceTests
    {
        private readonly IDisplayService _service;
        private readonly IChatMessageService _chat;
        public DisplayServiceTests(IDisplayService service, IChatMessageService chat)
        {
            _service = service;
            _chat = chat;
        }

        [KernelFunction("show")]
        [Description("Shows a modal to User with the provided title, body and action buttons.")]
        public async Task<TextContent> ShowModal(
            [Description("Title of the modal")]string title,
            [Description("Body content of the modal")]string body,
            [Description("Label for the acceptance button, default to 'Yes'")]string yes_button_text = "Yes",
            [Description("Label for the decline button, default to 'No'")]string no_button_text = "No")
        {
            await _chat.LogInfo($"Dialog request");
            var result = await _service.ShowModal(title, body, new BootstrapBlazor.Components.ResultDialogOption()
            {
                Size = BootstrapBlazor.Components.Size.Medium,
                ButtonYesText = yes_button_text,
                ButtonNoText = no_button_text
            });
            if (result == BootstrapBlazor.Components.DialogResult.Yes)
                return new TextContent($"User clicked {yes_button_text}");
            if (result == BootstrapBlazor.Components.DialogResult.No)
                return new TextContent($"User clicked {no_button_text}");
            return new TextContent($"User cancelled, neither {yes_button_text} nor {no_button_text} was clicked");
        }
    }
}
