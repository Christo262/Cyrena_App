using Cyrena.ArduinoIDE.Options;
using Cyrena.Contracts;
using Cyrena.Coding.Options;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.ArduinoIDE.Components.Shared
{
    public partial class Configure
    {
        [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
        [Parameter] public ChatConfiguration Model { get; set; } = default!;
        [Inject] private IFileDialog _win { get; set; } = default!;
        [Inject] private ISnackbar _snackbar { get; set; } = default!;
        private ArduinoConfig _model { get; set; } = default!;
        private MudForm _form = default!;

        protected override void OnInitialized()
        {
            _model = new ArduinoConfig()
            {
                InoPath = Model[ArduinoOptions.InoPath],
                Board = Model[ArduinoOptions.BoardId],
                ClockMhz = Model[ArduinoOptions.Clock],
                RamKb = Model[ArduinoOptions.Ram],
                Title = Model.Title,
                ConnectionId = Model.ConnectionId
            };
        }

        private async Task Submit()
        {
            await _form.ValidateAsync();
            if (!_form.IsValid) return;

            Model[ArduinoOptions.BoardId] = _model.Board;
            Model[ArduinoOptions.Clock] = _model.ClockMhz;
            Model[ArduinoOptions.Ram] = _model.RamKb;
            Model.Title = _model.Title;
            Model.ConnectionId = _model.ConnectionId!;
            MudDialog.Close(DialogResult.Ok(true));
        }

        private void Cancel() => MudDialog.Cancel();

        private async Task PickProject()
        {
            try
            {
                var files = await _win.OpenAsync("Choose ino file", ("ino", [".ino"]));
                if (!string.IsNullOrEmpty(files))
                {
                    var info = new FileInfo(files);
                    Model["ino"] = files;
                    _model.InoPath = files;
                    Model.WorkingDirectory = info.DirectoryName;
                }
            }
            catch (Exception ex)
            {
                _snackbar.Add(ex.Message, Severity.Error);
            }
        }
    }

    internal class ArduinoConfig
    {
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? InoPath { get; set; }
        [Required]
        public string? Board { get; set; }
        [Required]
        public string? RamKb { get; set; }
        [Required]
        public string? ClockMhz { get; set; }
        [Required]
        public string? ConnectionId { get; set; }
    }
}