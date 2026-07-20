using System.ComponentModel;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Screens.Contracts;
using Cyrena.Screens.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cyrena.Screens.Services;

/// <summary>
/// Kernel functions exposed to the AI. The screen-share service is
/// resolved from the kernel's service provider — one instance per
/// chat, so the AI in chat A always captures from chat A's source.
/// </summary>
internal class Functions
{
    private readonly IScreenInterop _screen;
    private readonly IIterationService _its;
    private readonly IChatMessageService _chat;
    private readonly IFileHandlerFactory _files;
    private readonly ScreenInteropModeService _mode;

    public Functions(
        IScreenInterop screen,
        IIterationService its,
        IChatMessageService chat,
        IFileHandlerFactory files,
        ScreenInteropModeService mode)
    {
        _screen = screen;
        _its = its;
        _chat = chat;
        _files = files;
        _mode = mode;
    }

    [KernelFunction("capture")]
    [Description("Captures a single frame from the user's shared screen")]
    public async Task<object> ScreenshotAsync()
    {
        if (!_screen.IsActive)
            return new ToolResult(false, "No screen is currently shared. Ask the user to share their screen first.");

        await _chat.LogInfo("Capturing screen...");

        var op = await _screen.CaptureAsync();
        if (!op.Success)
        {
            if (op.SourceLost == true)
                return new ToolResult(false, "The screen source was revoked. Ask the user to share again.");
            return new ToolResult(false, op.Error ?? "Capture failed.");
        }

        if (string.IsNullOrEmpty(op.DataUrl))
            return new ToolResult(false, "Capture returned no data.");

        // JS returned a data URL (data:image/png;base64,...). Strip the
        // header, decode, and route through IFileHandlerFactory so the
        // attachment is persisted alongside other chat files and gets
        // a stable ID. SaveAsync returns a KernelContent that's ready
        // to attach to the user message.
        var bytes = DecodeDataUrl(op.DataUrl);
        if (bytes is null)
            return new ToolResult(false, "Capture data was not a valid base64 data URL.");

        var fileName = op.FileName ?? "screenshot.png";
        var mimeType = op.MimeType ?? "image/png";

        KernelContent? attachment;
        try
        {
            attachment = await _files.SaveAsync(bytes, mimeType, fileName);
        }
        catch (Exception ex)
        {
            return new ToolResult(false, $"Saving the capture failed: {ex.Message}");
        }

        if (attachment is null)
            return new ToolResult(false, "File handler could not process the captured image.");

        if (_mode.Mode == InteropMode.UserMessage)
        {
            // Build the user message and add the captured frame as its
            // only item. The text portion is empty — the AI reads the
            // image directly. The user sees a pill in the chat just like
            // a paste.
            if (_its.Input == null)
                _its.Input = new ChatMessageContent(AuthorRole.User, fileName);
            if(string.IsNullOrEmpty(_its.Input.Content))
                _its.Input.Content = fileName;
            _its.Input.Items.Add(attachment);

            return new ToolResult(true, $"Captured {fileName} ({bytes.LongLength} bytes) and sent it as the next user message.");
        }
        
        return attachment;
    }

    private static byte[]? DecodeDataUrl(string dataUrl)
    {
        const string marker = ";base64,";
        var idx = dataUrl.IndexOf(marker, StringComparison.Ordinal);
        if (idx < 0) return null;
        var b64 = dataUrl[(idx + marker.Length)..];
        try { return Convert.FromBase64String(b64); }
        catch { return null; }
    }
}
