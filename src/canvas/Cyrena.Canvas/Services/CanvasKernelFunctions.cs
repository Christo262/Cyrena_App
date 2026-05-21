using Cyrena.Canvas.Contracts;
using Cyrena.Canvas.Models;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.Canvas.Services
{
    internal class CanvasKernelFunctions
    {
        private readonly ICanvasService _canvas;
        private readonly IChatMessageService _chat;
        public CanvasKernelFunctions(ICanvasService canvas, IChatMessageService chat)
        {
            _canvas = canvas;
            _chat = chat;
        }

        [KernelFunction("list")]
        [Description(
            "Lists all canvas documents that are currently available. " +
            "Use this when the user wants to see existing canvas documents, find a document to edit, or choose a document to activate.")]
        public async Task<IEnumerable<CanvasViewModel>> ListAsync(
            CancellationToken cancellationToken = default)
        {
            var items = await _canvas.ListAsync(cancellationToken);
            return items.Select(x => new CanvasViewModel(x));
        }

        [KernelFunction("activate")]
        [Description(
            "Activates an existing canvas document so it becomes the current document for reading and editing. " +
            "Call list first if the document id is not known. " +
            "After activation, use get_active to inspect the document with line numbers before making line-based edits.")]
        public async Task<ToolResult<CanvasDocumentLines>> ActivateAsync(
            [Description("The unique id of the canvas document to activate.")]
        string documentId,

            CancellationToken cancellationToken = default)
        {
            await _chat.LogInfo("Activating canvas...");
            if (!await _canvas.ActivateAsync(documentId, cancellationToken))
                return new ToolResult<CanvasDocumentLines>(false, $"Document with id '{documentId}' was not found.");

            return new ToolResult<CanvasDocumentLines>(
                new CanvasDocumentLines(_canvas.Current!));
        }

        [KernelFunction("get_active")]
        [Description(
            "Gets the currently active canvas document with line numbers. " +
            "Use this before calling write when you need to know exactly which lines to insert, replace, or remove. " +
            "This function does not modify the document.")]
        public ToolResult<CanvasDocumentLines> GetActive()
        {
            if (_canvas.Current == null)
                return new ToolResult<CanvasDocumentLines>(false, "No canvas document is currently active.");

            _chat.LogInfo("Reading canvas...");
            return new ToolResult<CanvasDocumentLines>(
                new CanvasDocumentLines(_canvas.Current));
        }

        [KernelFunction("create")]
        [Description(
            "Creates a new canvas document and automatically activates it for editing. " +
            "Use this when the user wants a new document, draft, note, code file, or other canvas item. " +
            "The created document becomes the active canvas document immediately.")]
        public async Task<ToolResult<CanvasDocument>> CreateAsync(
            [Description("The title of the new canvas document.")]
        string title,

            [Description("The type of canvas document to create.")]
        CanvasDocumentType documentType,

            CancellationToken cancellationToken = default)
        {
            await _chat.LogInfo($"Creating canvas {title}...");
            var doc = await _canvas.CreateAsync(title, documentType, cancellationToken);
            await _canvas.ActivateAsync(doc.Id, cancellationToken);

            return new ToolResult<CanvasDocument>(doc);
        }

        [KernelFunction("delete")]
        [Description(
            "Deletes a canvas document by id. " +
            "Use this only when the user clearly wants a document removed. " +
            "Call list first if the document id is not known.")]
        public async Task<ToolResult> DeleteAsync(
            [Description("The unique id of the canvas document to delete.")]
        string documentId,

            CancellationToken cancellationToken = default)
        {
            await _chat.LogInfo("Deleting canvas...");
            await _canvas.DeleteAsync(documentId, cancellationToken);
            return new ToolResult(true, $"Document with id '{documentId}' was deleted.");
        }

        [KernelFunction("write")]
        [Description(
            "Modifies the active canvas document by inserting or replacing lines. " +
            "The startLine parameter is zero-based. " +
            "If lineCount is 0, the content is inserted at startLine without removing any existing lines. " +
            "If lineCount is greater than 0, that many existing lines are removed starting at startLine, then the new content is inserted at the same position. " +
            "To replace the whole document, use startLine 0 and lineCount equal to the total number of existing lines. " +
            "To append to the end of the document, use startLine equal to the total number of existing lines and lineCount 0. " +
            "Use get_active first when you need to inspect line numbers before editing.")]
        public async Task<ToolResult<CanvasDocumentLines>> WriteAsync(
            [Description("The text content to insert or use as replacement content. You may style html documents using bootstrap css framework.")]
        string content,

            [Description("The zero-based line number where the insert or replacement should begin. Use 0 for the top of the document.")]
        int startLine = 0,

            [Description("The number of existing lines to remove before inserting the new content. Use 0 to insert without replacing anything.")]
        int lineCount = 0,

            CancellationToken cancellationToken = default)
        {
            try
            {
                await _chat.LogInfo("Updating canvas...");
                var doc = await _canvas.WriteAsync(
                    content,
                    startLine,
                    lineCount,
                    cancellationToken);

                return new ToolResult<CanvasDocumentLines>(
                    new CanvasDocumentLines(doc));
            }
            catch (Exception ex)
            {
                return new ToolResult<CanvasDocumentLines>(
                    false,
                    $"Failed to modify the active canvas document: {ex.Message}");
            }
        }
    }
}
