using Cyrena.Attributes;
using Cyrena.Coding.Contracts;
using Cyrena.Coding.Models;
using Cyrena.Extensions;
using Cyrena.Persistence.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Cyrena.Coding.Components.Shared
{
    public partial class DevelopPlanEditor
    {
        [KernelInject] private IDevelopPlanService _plan { get; set; } = null!;
        [KernelInject] private IStore<DynamicDevelopPlan> _plans { get; set; } = null!;
        [Inject] private IDialogService _dialog { get; set; } = null!;
        [Inject] private ISnackbar _toasts { get; set; } = null!;
        
        private DynamicDevelopPlan? _currentPlan;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            _currentPlan = await _plans.FindAsync(x => x.Id == _plan.Plan.Id);
            if (_currentPlan == null)
            {
                _currentPlan = new DynamicDevelopPlan()
                {
                    Id = _plan.Plan.Id,
                    AllowedFileTypes =  _plan.Plan.AllowedFileTypes,
                    IgnoredDirectories =  _plan.Plan.IgnoredDirectories,
                };
                foreach (var folder in _plan.Plan.Folders)
                {
                    var df = Traverse(_plan.Plan.Id, folder);
                    _currentPlan.Folders.Add(df);
                }
            }
            this.StateHasChanged();
        }

        private void RemoveFileType(string? e)
        {
            if (_currentPlan == null) return;
            if(!string.IsNullOrEmpty(e))
                _currentPlan.RemoveAllowedFile(e);
        }

        private async Task AddFileType()
        {
            if (_currentPlan == null) return;
            var type = await _dialog.ShowNameFormDialog("File Type", null);
            if(!string.IsNullOrEmpty(type))
                _currentPlan.AddAllowedFile(type);
        }

        private async Task AddFolder()
        {
            if (_currentPlan == null) return;
            var type = await _dialog.ShowNameFormDialog("Folder Name", null);
            if (!string.IsNullOrEmpty(type))
            {
                var folder = new DynamicDevelopFolder()
                {
                    Id = type.ToLower(),
                    Name = type,
                    AllowedFileTypes = new List<string>(_currentPlan.AllowedFileTypes),
                };
                _currentPlan.Folders.Add(folder);
            }
        }

        private void DeleteFolder(DynamicDevelopFolder folder)
        {
            if(_currentPlan != null && _currentPlan.Folders.Contains(folder))
                _currentPlan.Folders.Remove(folder);
        }

        private DynamicDevelopFolder Traverse(string planId, DevelopFolder folder)
        {
            var model = new DynamicDevelopFolder()
            {
                Id = folder.Id,
                Name = folder.Name,
                AllowedFileTypes = folder.AllowedFileTypes,
            };
            foreach (var item in folder.Folders)
            {
                var child = Traverse(planId, item);
                model.Children.Add(child);
            }
            return model;
        }

        private async Task SaveAsync()
        {
            if(_currentPlan == null) return; 
            await _plans.SaveAsync(_currentPlan);
            _toasts.Add("Develop Plan Saved", Severity.Success);
        }
    }   
}