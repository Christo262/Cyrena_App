using BootstrapBlazor.Components;
using Cyrena.Synthesis.Contracts;
using Cyrena.Synthesis.Models;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Synthesis.Components.Shared
{
    public partial class Index
    {
        [Inject] private ICapabilityStore _store { get; set; } = default!;
        [Inject] private ICapabilityPermissionService _permissionService { get; set; } = default!;
        [Inject] private DialogService _dialog { get; set; } = default!;

        private IEnumerable<DynamicCapability> _models { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            _models = await _store.GetAllAsync();
        }

        private async Task Delete(DynamicCapability model)
        {
            var result = await _dialog.ShowModal("Delete Capability", $"Are you sure you want to delete {model.Title}?", new ResultDialogOption()
            {
                Size = Size.Medium,
            });
            if(result == DialogResult.Yes)
            {
                await _store.DeleteAsync(model.Id);
                await _permissionService.DeleteAllPermissionsAsync(model.Id);
                _models = await _store.GetAllAsync();
            }
        }

        private async Task Edit(DynamicCapability model)
        {
            var result = await _dialog.ShowModal<CapEditForm>(new ResultDialogOption()
            {
                Size = Size.Medium,
                Title = "Edit Capability",
                ButtonYesText = "Submit",
                ButtonNoText = "Cancel",
                ComponentParameters = new()
                {
                    {"Model", model }
                }
            });
            if(result == DialogResult.Yes)
                await _store.SaveAsync(model);
            _models = await _store.GetAllAsync();
        }
    }
}
