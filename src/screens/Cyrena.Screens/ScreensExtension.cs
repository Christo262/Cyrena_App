using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;
using Cyrena.Screens.Contracts;
using Cyrena.Screens.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Screens;

public class ScreensExtension : Extension
{
    public override void BuildExtension(CyrenaBuilder builder)
    {
        builder.AddAssistantPlugin<AssistantPlugin>();
        builder.Services.AddSingleton<IScreenInterop, ScreenInterop>();
    }
}