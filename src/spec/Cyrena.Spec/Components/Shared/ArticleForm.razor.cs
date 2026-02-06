using BootstrapBlazor.Components;
using Cyrena.Spec.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Cyrena.Spec.Components.Shared
{
    public partial class ArticleForm : IResultDialog
    {
        [Parameter]
        public Article Model { get; set; } = default!;

        private EditContext _context = default!;

        private string? _keywords { get; set; }
        protected override void OnInitialized()
        {
            _context = new EditContext(Model);
            _keywords = string.Join(",", Model.Keywords);
        }

        Task IResultDialog.OnClose(DialogResult result)
        {
            return Task.CompletedTask;
        }

        async Task<bool> IResultDialog.OnClosing(DialogResult result)
        {
            if (result != DialogResult.Yes) return true;
            if(_keywords != null)
                Model.Keywords = _keywords.Split(",").Select(s => s.Trim()).ToList();
            var valid = _context.Validate();
            return valid;
        }
    }
}
