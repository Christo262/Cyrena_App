using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Developer.Options;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.Developer.Components.Shared
{
    public partial class DotnetCsConfig : IResultDialog
    {
        [Inject] private IFileDialog _file { get; set; } = default!;
        [Parameter] public ChatConfiguration Model { get; set; } = default!;
        private DotnetCsModel _model = default!;
        private EditContext _context = default!;
        protected override void OnInitialized()
        {
            _model = new DotnetCsModel()
            {
                Title = Model.Title,
                ConnectionId = Model.ConnectionId,
                ProjectFilePath = Model[DotnetOptions.ProjectFilePath],
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
                if (!File.Exists(_model.ProjectFilePath))
                    return false;
                Model.Title = _model.Title;
                Model.ConnectionId = _model.ConnectionId!;
                Model[DevelopOptions.RootDirectory] = new FileInfo(_model.ProjectFilePath).DirectoryName;
                Model[DotnetOptions.ProjectFilePath] = _model.ProjectFilePath;
            }
            return valid;
        }

        private async Task ChooseProj()
        {
            var f = await _file.OpenAsync("Choose .csproj", ("csproj", [".csproj"]));
            _model.ProjectFilePath = f;
        }
    }

    internal class DotnetCsModel
    {
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? ConnectionId { get; set; }
        [Required]
        public string? ProjectFilePath { get; set; }
    }
}
