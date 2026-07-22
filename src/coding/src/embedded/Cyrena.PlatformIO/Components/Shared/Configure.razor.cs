using Cyrena.PlatformIO.Options;
using Cyrena.Contracts;
using Cyrena.Coding.Options;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Cyrena.PlatformIO.Components.Shared
{
    public partial class Configure
    {
        [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
        [Parameter] public ChatConfiguration Model { get; set; } = default!;
        [Inject] private IFileDialog _win { get; set; } = default!;
        [Inject] private ISnackbar _snackbar { get; set; } = default!;
        private PlatformIOConfig _model { get; set; } = default!;
        private MudForm _form = default!;

        protected override void OnInitialized()
        {
            _model = new PlatformIOConfig()
            {
                IniPath = Model[PlatformIOOptions.IniFile],
                Environment = Model[PlatformIOOptions.Environment],
                Title = Model.Title,
                ConnectionId = Model.ConnectionId
            };
        }

        private async Task Submit()
        {
            await _form.ValidateAsync();
            if (!_form.IsValid) return;

            var dir = Path.GetDirectoryName(_model.IniPath);
            Model.WorkingDirectory = dir;
            Model[PlatformIOOptions.Environment] = _model.Environment;
            Model[PlatformIOOptions.IniFile] = _model.IniPath;

            Model.Title = _model.Title;
            Model.ConnectionId = _model.ConnectionId!;
            MudDialog.Close(DialogResult.Ok(true));
        }

        private void Cancel() => MudDialog.Cancel();

        private async Task PickProject()
        {
            try
            {
                var files = await _win.OpenAsync("Choose platformio.ini", ("ini", [".ini"]));
                if (!string.IsNullOrEmpty(files))
                {
                    var info = new FileInfo(files);
                    Model["ini"] = files;
                    _model.IniPath = files;
                    Model.WorkingDirectory = info.DirectoryName;
                }
            }
            catch (Exception ex)
            {
                _snackbar.Add(ex.Message, Severity.Error);
            }
        }
    }

    internal class PlatformIOConfig
    {
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? IniPath { get; set; }
        [Required]
        public string? Environment { get; set; }
        [Required]
        public string? ConnectionId { get; set; }
    }
}