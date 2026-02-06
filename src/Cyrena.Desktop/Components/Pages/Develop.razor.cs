using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Spec.Components.Shared;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Desktop.Components.Pages
{
    public partial class Develop : IDeveloperWindow
    {
        [Parameter] public string? Id { get; set; }
        [Inject] private ToastService _toasts { get; set; } = default!;
        [Inject] private IProjectLoader _loader { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;
        [Inject] private ICurrentWindow _win { get; set;  } = default!;

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
                _context = await _loader.LoadProjectAsync(Id);
                BuildMenu();
                _win.Restore();
                this.StateHasChanged();
            }
            catch (Exception ex)
            {
                await _toasts.Error("Error Loading", ex.Message);
                _nav.NavigateTo("");
            }
        }

        private List<ProjectFile> _openFiles { get; set; } = new();

        public void OpenFile(string fileId)
        {
            if (_context == null) return;
            if (_context.ProjectPlan.TryFindFile(fileId, out var file))
            {
                ToggleFs();
                _openFiles.Add(file!);
                this.StateHasChanged();
            }
        }

        private Task<bool> OnTabClose(TabItem item)
        {
            var i = _openFiles.FirstOrDefault(x => x.Name == item.Id);
            if (i != null) _openFiles.Remove(i);
            this.StateHasChanged();
            return Task.FromResult(true);
        }

        private bool _fs;
        private void ToggleFs() => _fs = !_fs;

        private List<MenuItem>? _menuItems { get; set; }

        private void BuildMenu()
        {
            if(_context == null) return;
            _menuItems = new List<MenuItem>();
            foreach (var item in _context.ProjectPlan.Folders)
            {
                _menuItems.Add(Build(item));
            }

            foreach (var item in _context.ProjectPlan.Files)
            {
                var ffm = new MenuItem() { Text = item.Name, Id = item.Id, Icon = "bi bi-file-code text-success" };
                _menuItems.Add(ffm);
            }
        }

        private MenuItem Build(ProjectFolder folder)
        {
            var model = new MenuItem() { Text = folder.Name, Icon = "bi bi-folder text-warning" };
            var modelItems = new List<MenuItem>();
            foreach (var item in folder.Folders)
            {
                var fm = Build(item);
                modelItems.Add(fm);
            }

            foreach (var item in folder.Files)
            {
                var ffm = new MenuItem() { Text = item.Name, Id = item.Id, Icon = "bi bi-file-code text-success" };
                modelItems.Add(ffm);
            }
            model.Items = modelItems;
            return model;
        }

        private Task MenuItemClick(MenuItem item)
        {
            if(!string.IsNullOrEmpty(item.Id))
                OpenFile(item.Id);
            return Task.CompletedTask;
        }

        private ArticleViewer _articles = default!;
    }
}
