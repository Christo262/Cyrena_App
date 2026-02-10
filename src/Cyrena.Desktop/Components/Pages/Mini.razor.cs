using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Spec.Components.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Desktop.Components.Pages
{
    public partial class Mini : IDeveloperWindow
    {
        [Parameter] public string? Id { get; set; }
        [Inject] private ToastService _toasts { get; set; } = default!;
        [Inject] private IProjectLoader _loader { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;
        [Inject] private ICurrentWindow _win { get; set; } = default!;

        private IDeveloperContext? _context { get; set; }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            if (string.IsNullOrEmpty(Id))
            {
                _nav.NavigateTo("");
                await _toasts.Error("Error", "No project id");
                return;
            }
            try
            {
                _context = await _loader.LoadProjectAsync(Id, ctx => ctx.Services.AddSingleton<IDeveloperWindow>(this));
                _win.SetHeight(800);
                _win.SetWidth(400);
                _win.SetTitle(_context.Project.Name);
                this.StateHasChanged();
            }
            catch (Exception ex)
            {
                await _toasts.Error("Error Loading", ex.Message);
                _nav.NavigateTo("");
            }
        }

        public void OpenFile(string fileId)
        {
        }

        public void FilesChanged()
        {
        }

        private ArticleViewer _articles = default!;
    }
}
