using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Components.Shared
{
    public partial class CustomizationSettings
    {
        [Inject] private ISettingsService _settings { get; set; } = default!;
        [Inject] private IJSRuntime _js { get; set; } = default!;
        [Inject] private ToastService _toasts { get; set; } = default!;
        [Inject] private HeadOutletStateChangeTracker _head { get; set; } = default!;
        [Inject] private IServiceProvider _services { get; set; } = default!;

        private Customization _model = default!;
        private InputFile _fileInput = null!;
        private IEnumerable<ViewStart> _view_starts = [];

        private int _bg_opa { get; set; }

        protected override void OnInitialized()
        {
            _model = _settings.Read<Customization>(Customization.Key) ?? new Customization();
            _bg_opa = (int)(_model.Background.BackgroundOpacity * 100f);
            var viewstarts = _services.GetServices<IViewStartProvider>();
            var vms = new List<ViewStart>();
            foreach (var view in viewstarts)
                vms.AddRange(view.Provide());
            _view_starts = vms;
        }

        private async Task TriggerFileUpload()
        {
            await _js.InvokeVoidAsync("triggerClick", _fileInput.Element);
        }

        private void BackgroundColorChanged(string color)
        {
            _model.Background.BackgroundColor = color;
            SaveCustoms();
        }

        private void BackgroundOpacityChanged(int value)
        {
            _bg_opa = value;
            _model.Background.BackgroundOpacity = (float)(value / 100f);
            SaveCustoms();
        }

        private async Task HandleFilesSelected(InputFileChangeEventArgs e)
        {
            try
            {
                using var fs = e.File.OpenReadStream(10 * 1024 * 1024);
                using var ms = new MemoryStream();
                await fs.CopyToAsync(ms);
                ms.Position = 0;
                var dir = Path.Combine(CyrenaBuilder.AppDataDirectory, "public", "wallpapers");
                if(Directory.Exists(dir))
                    Directory.Delete(dir, true);
                if(!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, e.File.Name), ms.ToArray());
                _model.Background.BackgroundImage = $"wallpapers/{e.File.Name}";
                SaveCustoms();
            }catch(Exception ex)
            {
                await _toasts.Error($"{e.File.Name} Error", ex.Message);
            }
        }

        private void ResetToDefault()
        {
            var mascot = _model.Mascot;
            _model = new Customization()
            {
                Mascot = mascot,
            };
            _bg_opa = (int)(_model.Background.BackgroundOpacity * 100f);
            SaveCustoms() ;
        }

        private void NoWallpaper()
        {
            _model.Background.BackgroundImage = null;
            SaveCustoms();
        }

        private void SaveCustoms()
        {
            _settings.Save(Customization.Key, _model);
            _head.Invoke();
        }

        private void SaveCustomsSilent()
        {
            _settings.Save(Customization.Key, _model);
        }
    }
}
