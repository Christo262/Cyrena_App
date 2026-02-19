using BootstrapBlazor.Components;
using Cyrena.ArduinoIDE.Options;
using Cyrena.Contracts;
using Cyrena.Developer.Options;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Cyrena.ArduinoIDE.Components.Shared
{
    public partial class Configure : IResultDialog
    {
        [Parameter] public ChatConfiguration Model { get; set; } = default!;
        [Inject] private IFileDialog _win { get; set; } = default!;
        [Inject] private ToastService _toasts { get;set;  } = default!;
        private ArduinoConfig _model { get; set; } = default!;
        private EditContext _context = default!;

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
            _context = new EditContext(_model);
        }

        Task IResultDialog.OnClose(DialogResult result)
        {
            return Task.CompletedTask;
        }

        async Task<bool> IResultDialog.OnClosing(DialogResult result)
        {
            if (result != DialogResult.Yes) return true;
            var valid = _context.Validate();
            if (valid)
            {
                Model[ArduinoOptions.BoardId] = _model.Board;
                Model[ArduinoOptions.Clock] = _model.ClockMhz;
                Model[ArduinoOptions.Ram] = _model.RamKb;
                Model.Title = _model.Title;
                Model.ConnectionId = _model.ConnectionId!;
            }
            return valid;
        }

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
                    Model[DevelopOptions.RootDirectory] = info.DirectoryName;
                }
            }
            catch (Exception ex)
            {
                await _toasts.Error("Error", ex.Message);
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
