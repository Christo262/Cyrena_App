using Cyrena.Contracts;
using Cyrena.Extensions;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.Runtime.Services
{
    internal class AttachmentKernelFunctions
    {
        private readonly IFileHandlerFactory _files;
        private readonly IChatMessageService _chat;
        public AttachmentKernelFunctions(IFileHandlerFactory files, IChatMessageService chat)
        {
            _files = files;
            _chat = chat;
        }

        [KernelFunction("get")]
        [Description("File attachments are not directly available in user's messages, you will only see a FileReferenceContent that contains a file_name. This function Retrieves the content of an attached file by file_name. Use this when the user asks about, summarizes, analyzes, converts, or references an attached file.")]
        public async Task<KernelContent> GetFileAsync(
            [Description("The file name from the file reference attached to the message.")]
            string file_name,
            CancellationToken cancellationToken = default)
        {
            await _chat.LogInfo("Retrieving attachment...");
            var content = await _files.GetKernelContent(file_name, cancellationToken);
            return content;
        }
    }
}
