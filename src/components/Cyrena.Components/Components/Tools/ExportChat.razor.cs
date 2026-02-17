using Cyrena.Contracts;
using Cyrena.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Components.Tools
{
    public partial class ExportChat
    {
        private IChatMessageService _chat = default!;
        [Inject] private IFileDialog _file { get; set; } = default!;

        protected override void OnInitialized()
        {
            _chat = Kernel.Services.GetRequiredService<IChatMessageService>();
        }

        private async Task ExportChatAsync()
        {
            var path = await _file.ShowSaveFile("Export Chat", ("txt", [".txt"]));
            if (string.IsNullOrEmpty(path))
                return;
            if (!path.EndsWith(".txt"))
                path += ".txt";
            var sb = new System.Text.StringBuilder();
            foreach(var item in _chat.DisplayHistory)
            {
                sb.AppendLine($"[{item.Role.Label}]");
                sb.AppendLine(item.Content);
                sb.AppendLine();
            }

            try
            {
                await File.WriteAllTextAsync(path, sb.ToString());
            }
            catch (Exception ex)
            {
                await _chat.LogError("Export failed: " + ex);
            }
        }
    }
}
