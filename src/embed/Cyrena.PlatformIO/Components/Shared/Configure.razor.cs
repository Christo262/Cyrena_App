using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Developer.Options;
using Cyrena.Models;
using Cyrena.PlatformIO.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.PlatformIO.Components.Shared
{
    public partial class Configure : IResultDialog
    {
        [Parameter] public ChatConfiguration Model { get; set; } = default!;
        [Inject] private IFileDialog _win { get; set; } = default!;
        [Inject] private ToastService _toasts { get; set; } = default!;

        private PioConfig _model = default!;
        private EditContext _context = default!;

        protected override void OnInitialized()
        {
            _model = new PioConfig()
            {
                Title = Model.Title,
                ConnectionId = Model.ConnectionId,
                IniFilePath = Model[PlatformIOOptions.IniFile]
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
                Model.Title = _model.Title;
                Model.ConnectionId = _model.ConnectionId!;
            }
            return valid;
        }

        private async Task PickProject()
        {
            try
            {
                var files = await _win.OpenAsync("Choose ini file", ("ini", [".ini"]));
                if (files != null)
                {
                    var info = new FileInfo(files);
                    Model[DevelopOptions.RootDirectory] = info.DirectoryName ?? string.Empty;
                    Model[PlatformIOOptions.IniFile] = files;
                    _model.IniFilePath = files;
                }
            }
            catch (Exception ex)
            {
                await _toasts.Error("Error", ex.Message);
            }
        }
    }

    internal class PioConfig
    {
        [Required]
        public string? IniFilePath { get; set; }
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? ConnectionId { get; set; }
    }
}
