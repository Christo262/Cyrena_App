using Cyrena.Coding.Contracts;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Options;
using Cyrena.VisualStudio.Contracts;
using Cyrena.VisualStudio.Models;
using Cyrena.VisualStudio.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace Cyrena.VisualStudio.Extensions;

public static class CyrenaBuilderExtensions
{
    public static void AddProjectHandler<TProjHandler>(this CyrenaBuilder builder)
        where TProjHandler : class, IProjHandler
    {
        builder.Services.AddSingleton<IProjHandler, TProjHandler>();
        builder.Services.AddScoped<IShortcut>(sp =>
        {
            var projs = sp.GetServices<IProjHandler>();
            var proj =  projs.First(x => x is TProjHandler);
            var dialog = sp.GetRequiredService<IDialogService>();
            var kernel = sp.GetRequiredService<IKernelController>();
            var nav = sp.GetRequiredService<NavigationManager>();
            return new ProjectShortcut(dialog, kernel, nav, proj);
        });

        builder.Services.AddSingleton<ICodeBuilder>(sp =>
        {
            var projs = sp.GetServices<IProjHandler>();
            var proj =  projs.First(x => x is TProjHandler);
            var kernel = sp.GetRequiredService<IKernelController>();
            return new ProjectCodeBuilder(kernel, proj);
        });
    }
}