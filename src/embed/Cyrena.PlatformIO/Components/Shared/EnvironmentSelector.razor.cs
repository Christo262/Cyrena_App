using BootstrapBlazor.Components;
using Cyrena.PlatformIO.Contracts;
using Cyrena.PlatformIO.Models;
using Microsoft.AspNetCore.Components;

namespace Cyrena.PlatformIO.Components.Shared
{
    public partial class EnvironmentSelector : IResultDialog
    {
        [Parameter] public IEnvironmentController Controller { get; set; } = default!;

        Task IResultDialog.OnClose(DialogResult result)
        {
            return Task.CompletedTask;
        }

        async Task<bool> IResultDialog.OnClosing(DialogResult result)
        {
            return true;
        }

        private void SetEnvironment(PlatformIOEnvironment env)
        {
            Controller.SetCurrentEnvironment(env.Name);
        }
    }
}
