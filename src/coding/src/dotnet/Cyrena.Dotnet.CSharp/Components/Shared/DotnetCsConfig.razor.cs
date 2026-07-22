using Cyrena.Coding.Options;
using Cyrena.Contracts;
using Cyrena.Dotnet.Options;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.Dotnet.CSharp.Components.Shared
{
    public partial class DotnetCsConfig
    {
        [Inject] private IDialogService _dialog { get; set; } = default!;
        [Inject] private IFileDialog _file { get; set; } = default!;
        [Inject] private ISnackbar _snackbar { get; set; } = default!;
        [Parameter] public ChatConfiguration Model { get; set; } = default!;
        [CascadingParameter] private IMudDialogInstance? _mudDialog { get; set; }
        private DotnetCsModel _model = default!;
        private MudForm _form = default!;

        protected override void OnInitialized()
        {
            _model = new DotnetCsModel()
            {
                Title = Model.Title,
                ConnectionId = Model.ConnectionId,
                ProjectFilePath = Model[DotnetOptions.ProjectFilePath],
            };
        }

        private async Task ChooseProj()
        {
            var f = await _file.OpenAsync("Choose .csproj", ("csproj", [".csproj"]));
            if (f != null)
                _model.ProjectFilePath = f;
        }

        private async Task Submit()
        {
            await _form.ValidateAsync();
            if (!_form.IsValid)
                return;

            if (!File.Exists(_model.ProjectFilePath))
            {
                _snackbar.Add("Project file not found", Severity.Error);
                return;
            }

            Model.Title = _model.Title;
            Model.ConnectionId = _model.ConnectionId ?? string.Empty;
            Model.WorkingDirectory = new FileInfo(_model.ProjectFilePath).DirectoryName ?? string.Empty;
            Model[DotnetOptions.ProjectFilePath] = _model.ProjectFilePath;
            _mudDialog?.Close(DialogResult.Ok(true));
        }

        private void Cancel() => _mudDialog?.Cancel();
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