using BlazorMonaco.Editor;
using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Developer.Contracts;
using Cyrena.Developer.Extensions;
using Cyrena.Developer.Models;
using Cyrena.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.SemanticKernel;

namespace Cyrena.Developer.Components.Pages
{
    public partial class DevelopFileViewer : IAsyncDisposable
    {
        [Parameter] public string? KernelId { get; set; }
        [Parameter] public string? FileId { get; set; }
        [Inject] private IKernelController _controller { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;
        [Inject] private ToastService _toasts { get; set;  } = default!;
        [Inject] private IJSRuntime _js { get; set; } = default!;

        private Kernel? _kernel { get; set; }
        private DevelopFileContent? _original { get; set; }
        private DevelopFileContent? _current { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            if(string.IsNullOrEmpty(KernelId) || string.IsNullOrEmpty(FileId))
            {
                _nav.NavigateTo("");
                return;
            }

            try
            { 
                _kernel = await _controller.LoadAsync(KernelId);
                var versionControl = _kernel.Services.GetService<IVersionControl>();
                if (versionControl == null)
                    throw new NullReferenceException("No version control service found.");
                IDevelopPlanService plan = _kernel.GetRequiredService<IDevelopPlanService>();
                _original = versionControl.GetBackups(FileId);
                if(plan.Plan.TryFindFile(FileId, out var file))
                {
                    plan.Plan.TryReadFileContent(file!, out var co);
                    _current = co;
                }
                this.StateHasChanged();

            }catch (Exception ex)
            {
                await _toasts.Error("Error", ex.Message);
                _nav.NavigateTo("");
            }
        }

        private StandaloneDiffEditor _diffEditor = default!;
        TextModel? originalModel = null;
        TextModel? modifiedModel = null;
        private async Task EditorOnDidInit()
        {
            if (_original != null)
            {
                var ext = Path.GetExtension(_original.RelativePath);
                var lang = _langs.GetFileLanguage(ext);
                originalModel = await BlazorMonaco.Editor.Global.CreateModel(_js, _original.Content, lang, $"{FileId}-originalModel");
            }

            // Get or create the modified model
            if (_current != null)
            {
                var ext = Path.GetExtension(_current.RelativePath);
                var lang = _langs.GetFileLanguage(ext);
                modifiedModel = await BlazorMonaco.Editor.Global.CreateModel(_js, _current.Content, lang, $"{FileId}-modifiedModel");
            }

            // Set the editor model
            if (_diffEditor == null)
                return;
            await _diffEditor.SetModel(new DiffEditorModel
            {
                Original = originalModel,
                Modified = modifiedModel
            });
        }

        private StandaloneDiffEditorConstructionOptions DiffEditorConstructionOptions(StandaloneDiffEditor editor)
        {
            return new StandaloneDiffEditorConstructionOptions
            {
                OriginalEditable = false,
                Theme = "vs-dark"
            };
        }

        private void Revert()
        {
            if (_original == null || _kernel == null) return;
            IDevelopPlanService plan = _kernel.GetRequiredService<IDevelopPlanService>();
            if (!plan.Plan.TryWriteFileContent(_original, _original.Content, out var _))
                _toasts.Warning("Error", "Something went wrong trying to revert");
            else
            {
                var versionControl = _kernel.Services.GetRequiredService<IVersionControl>();
                versionControl.RemoveBackup(_original.Id);
                _nav.NavigateTo($"converse/{KernelId}");
            }
        }

        private void Keep()
        {
            if (string.IsNullOrEmpty(FileId) || _kernel == null) return;
            var versionControl = _kernel.Services.GetRequiredService<IVersionControl>();
            versionControl.RemoveBackup(FileId);
            _nav.NavigateTo($"converse/{KernelId}");
        }

        public async ValueTask DisposeAsync()
        {
            if (originalModel != null)
                await originalModel.DisposeModel();
            if(modifiedModel != null)
                await modifiedModel.DisposeModel();
            if (_diffEditor != null)
                await _diffEditor.DisposeEditor();
        }

        private CodeLanguages _langs = new CodeLanguages();
    }
}
