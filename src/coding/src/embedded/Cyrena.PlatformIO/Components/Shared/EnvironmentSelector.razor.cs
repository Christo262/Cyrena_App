using Cyrena.Coding.Options;
using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.PlatformIO.Contracts;
using Cyrena.PlatformIO.Models;
using Cyrena.PlatformIO.Options;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Cyrena.PlatformIO.Components.Shared
{
    public partial class EnvironmentSelector
    {
        [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
        [Parameter] public IEnvironmentController Controller { get; set; } = default!;

        private void SetEnvironment(PlatformIOEnvironment env)
        {
            Controller.SetCurrentEnvironment(env.Name);
        }

        private void Cancel() => MudDialog.Cancel();
    }
}