using Cyrena.PlatformIO.Options;
using Cyrena.Contracts;
using Cyrena.Coding.Options;
using Cyrena.Models;
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
        [Parameter] public string? Value { get; set; }
        [Parameter] public EventCallback<string?> ValueChanged { get; set; }
        [Inject] private ISnackbar _snackbar { get; set; } = default!;
        private List<string> _environments = new();

        protected override void OnInitialized()
        {
            try
            {
                var iniPath = Value;
                if (!string.IsNullOrEmpty(iniPath) && File.Exists(iniPath))
                {
                    var lines = File.ReadAllLines(iniPath);
                    _environments = lines
                        .Where(l => l.TrimStart().StartsWith("[env:") && l.Contains("]"))
                        .Select(l => l.Trim().Substring(5, l.Trim().IndexOf(']') - 5))
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _snackbar.Add(ex.Message, Severity.Error);
            }
        }

        private void SelectEnvironment(string env)
        {
            Value = env;
            ValueChanged.InvokeAsync(env);
            MudDialog.Close(DialogResult.Ok(env));
        }

        private void Cancel() => MudDialog.Cancel();
    }
}