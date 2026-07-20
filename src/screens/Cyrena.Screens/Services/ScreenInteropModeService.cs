using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Screens.Models;

namespace Cyrena.Screens.Services;

public class ScreenInteropModeService
{
    private readonly IPromptManager _prompts;

    public ScreenInteropModeService(IPromptManager prompts)
    {
        _prompts = prompts;
    }
    
    public InteropMode Mode { get; private set; } = InteropMode.UserMessage;
    
    public void SetInteropMode(InteropMode mode)
    {
        Mode = mode;
        string prompt;
        if (Mode == InteropMode.UserMessage)
            prompt = Resources.Read(typeof(ScreenInterop).Assembly, "Cyrena.Screens.Resources.usr-msg-prompt.md");
        else
            prompt = Resources.Read(typeof(ScreenInterop).Assembly, "Cyrena.Screens.Resources.fn-res-prompt.md");
        if (string.IsNullOrEmpty(_promptId))
            _promptId = _prompts.AddPrompt(20, prompt);
        else
            _prompts.UpdatePrompt(_promptId, prompt);
    }
    
    private string? _promptId { get; set; }
}