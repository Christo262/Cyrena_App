using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Options;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.Canvas.Services
{
    internal class CanvasImageKernelFunctions
    {
        private readonly IFileHandlerFactory _files;
        private readonly IChatMessageService _chat;
        public CanvasImageKernelFunctions(IFileHandlerFactory files, IChatMessageService chat)
        {
            _files = files;
            _chat = chat;
        }

        [KernelFunction("get_image_path")]
        [Description("Gets the path to a image file User has attached for usage in Canvas documents.")]
        public async Task<ToolResult<string>> GetImagePath(
            [Description("The file name from the file reference attached to the message.")]string file_name,
            CancellationToken cancellationToken = default)
        {
            var att = await _files.GetAttachmentAsync(file_name, cancellationToken);
            if (att == null || !File.Exists(att.Path)) return new ToolResult<string>(false, "File not found");
            if (!att.MimeType.StartsWith("image/")) return new ToolResult<string>(false, "Not an image");
            await _chat.LogInfo($"Inserting image...");
            var path = att.Path.Replace(CyrenaBuilder.ConversationsData, "").Replace("\\", "/").TrimStart('/');
            return new ToolResult<string>(path, true);
        }
    }
}
