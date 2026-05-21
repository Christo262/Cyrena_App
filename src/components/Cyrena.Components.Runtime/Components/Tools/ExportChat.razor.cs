using Cyrena.Attributes;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using System.Text.Json;

namespace Cyrena.Components.Tools
{
    public partial class ExportChat
    {
        [KernelInject] private IChatMessageService _chat { get; set; } = default!;
        [Inject] private IFileDialog _file { get; set; } = default!;

        private async Task ExportChatAsync()
        {
            var path = await _file.ShowSaveFileAsync("Export Chat", ("txt", [".txt"]));
            if (string.IsNullOrEmpty(path))
                return;
            if (!path.EndsWith(".txt"))
                path += ".txt";
            var sb = new System.Text.StringBuilder();
            foreach(var item in _chat.KernelHistory)
            {
                var json = JsonSerializer.Serialize(item, new JsonSerializerOptions() { WriteIndented = true });
                sb.AppendLine(json);
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
