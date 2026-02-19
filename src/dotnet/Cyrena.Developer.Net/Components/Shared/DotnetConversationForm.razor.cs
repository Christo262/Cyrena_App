using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Developer.Options;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.Developer.Components.Shared
{
    public partial class DotnetConversationForm : IResultDialog
    {
        [Inject] private IFileDialog _file { get; set; } = default!;
        [Parameter] public ChatConfiguration Configuration { get; set; } = default!;
        private SolutionConfig _model = default!;
        private EditContext _context = default!;
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
                if(!File.Exists(_model.SolutionFilePath))
                    return false;
                Configuration.Title = _model.Title;
                Configuration.ConnectionId = _model.ConnectionId!;
                Configuration[DevelopOptions.RootDirectory] = new FileInfo(_model.SolutionFilePath).DirectoryName;
                Configuration[DotnetOptions.SolutionFilePath] = _model.SolutionFilePath;
            }
            return valid;
        }

        private async Task ChooseSln()
        {
            var f = await _file.OpenAsync("Choose .NET Solution", ("sln", [".sln", ".slnx"]));
            _model.SolutionFilePath = f;
        }

        public class SolutionConfig
        {
            [Required]
            public string? Title { get; set; }
            [Required]
            public string? ConnectionId { get; set; }
            [Required]
            public string? SolutionFilePath { get; set; }
        }
    }
}
