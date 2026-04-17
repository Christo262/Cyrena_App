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
        [Inject] private ToastService _toasts { get; set; } = default!;
        [Inject] private IJSRuntime _js { get; set; } = default!;

        private Kernel? _kernel { get; set; }
        private IReadOnlyList<DevelopFileVersion>? _originals { get; set; }
        private DevelopFileContent? _current { get; set; }
        private int _selectedVersionIndex { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            if (string.IsNullOrEmpty(KernelId) || string.IsNullOrEmpty(FileId))
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
                _originals = versionControl.GetHistory(FileId);
                if (plan.Plan.TryFindFile(FileId, out var file))
                {
                    plan.Plan.TryReadFileContent(file!, out var co);
                    _current = co;
                }

                // Default to the latest version
                if (_originals != null && _originals.Count > 0)
                {
                    _selectedVersionIndex = _originals.Count - 1;
                    _og_target = _originals[_selectedVersionIndex];
                }

                this.StateHasChanged();
            }
            catch (Exception ex)
            {
                await _toasts.Error("Error", ex.Message);
                _nav.NavigateTo("");
            }
        }

        private StandaloneDiffEditor _diffEditor = default!;
        TextModel? originalModel = null;
        TextModel? modifiedModel = null;
        private DevelopFileVersion? _og_target { get; set; }

        private async Task OnVersionSelected(SelectedItem item)
        {
            if (_originals == null || _diffEditor == null) return;
            if (!int.TryParse(item.Value, out var index)) return;

            _selectedVersionIndex = index;
            _og_target = _originals[index];

            // Dispose and recreate the original model with the selected version's content
            if (originalModel != null)
                await originalModel.DisposeModel();

            var ext = Path.GetExtension(_og_target.File.RelativePath);
            var lang = _langs.GetFileLanguage(ext);
            originalModel = await BlazorMonaco.Editor.Global.CreateModel(_js, _og_target.File.Content, lang, $"{FileId}-originalModel-{index}");

            await _diffEditor.SetModel(new DiffEditorModel
            {
                Original = originalModel,
                Modified = modifiedModel
            });
        }

        private async Task OnVersionSelected()
        {
            if (_originals == null || _diffEditor == null) return;
            
            _og_target = _originals[_selectedVersionIndex];

            // Dispose and recreate the original model with the selected version's content
            if (originalModel != null)
                await originalModel.DisposeModel();

            var ext = Path.GetExtension(_og_target.File.RelativePath);
            var lang = _langs.GetFileLanguage(ext);
            originalModel = await BlazorMonaco.Editor.Global.CreateModel(_js, _og_target.File.Content, lang, $"{FileId}-originalModel-{_selectedVersionIndex}");

            await _diffEditor.SetModel(new DiffEditorModel
            {
                Original = originalModel,
                Modified = modifiedModel
            });
        }

        private async Task EditorOnDidInit()
        {
            if (_originals != null && _originals.Count > 0)
            {
                _og_target = _originals[_selectedVersionIndex];
                var ext = Path.GetExtension(_og_target.File.RelativePath);
                var lang = _langs.GetFileLanguage(ext);
                originalModel = await BlazorMonaco.Editor.Global.CreateModel(_js, _og_target.File.Content, lang, $"{FileId}-originalModel");
            }

            if (_current != null)
            {
                var ext = Path.GetExtension(_current.RelativePath);
                var lang = _langs.GetFileLanguage(ext);
                modifiedModel = await BlazorMonaco.Editor.Global.CreateModel(_js, _current.Content, lang, $"{FileId}-modifiedModel");
            }

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
            if (_og_target == null || _kernel == null) return;
            IDevelopPlanService plan = _kernel.GetRequiredService<IDevelopPlanService>();
            if (!plan.Plan.TryWriteFileContent(_og_target.File, _og_target.File.Content, out var _))
                _toasts.Warning("Error", "Something went wrong trying to revert");
            else
            {
                var versionControl = _kernel.Services.GetRequiredService<IVersionControl>();
                versionControl.RollbackTo(_og_target);
                _og_target = null;
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
            if (modifiedModel != null)
                await modifiedModel.DisposeModel();
            if (_diffEditor != null)
                await _diffEditor.DisposeEditor();
        }

        private CodeLanguages _langs = new CodeLanguages();
    }
}