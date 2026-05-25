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
    public partial class DotnetConversationForm
    {
        [Inject] private IDialogService _dialog { get; set; } = default!;
        [Inject] private IFileDialog _file { get; set; } = default!;
        [Inject] private ISnackbar _snackbar { get; set; } = default!;
        [Parameter] public ChatConfiguration Configuration { get; set; } = default!;
        [CascadingParameter] private IMudDialogInstance? _mudDialog { get; set; }
        private SolutionConfig _model = default!;
        private EditContext _context = default!;
        private MudForm _form = default!;

        protected override void OnInitialized()
        {
            _model = new SolutionConfig()
            {
                Title = Configuration.Title,
                ConnectionId = Configuration.ConnectionId,
                SolutionFilePath = Configuration[DotnetOptions.SolutionFilePath],
            };
            _context = new EditContext(_model);
        }

        private async Task ChooseSln()
        {
            var f = await _file.OpenAsync("Choose .NET Solution", ("sln", [".sln", ".slnx"]));
            if (f != null)
                _model.SolutionFilePath = f;
        }

        private async Task Submit()
        {
            await _form.ValidateAsync();
            if (!_form.IsValid)
                return;

            if (!File.Exists(_model.SolutionFilePath))
            {
                _snackbar.Add("Solution file not found", Severity.Error);
                return;
            }

            Configuration.Title = _model.Title;
            Configuration.ConnectionId = _model.ConnectionId ?? string.Empty;
            Configuration.WorkingDirectory = new FileInfo(_model.SolutionFilePath).DirectoryName ?? string.Empty;
            Configuration[DotnetOptions.SolutionFilePath] = _model.SolutionFilePath;
            _mudDialog?.Close(DialogResult.Ok(_model));
        }

        private void Cancel() => _mudDialog?.Cancel();
    }

    internal class SolutionConfig
    {
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? ConnectionId { get; set; }
        [Required]
        public string? SolutionFilePath { get; set; }
    }
}