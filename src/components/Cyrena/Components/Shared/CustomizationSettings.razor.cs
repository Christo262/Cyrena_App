using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Components.Shared
{
    public partial class CustomizationSettings
    {
        [Inject] private ISettingsService _settings { get; set; } = default!;
        [Inject] private IJSRuntime _js { get; set; } = default!;
        [Inject] private ISnackbar _snackbar { get; set; } = default!;
        [Inject] private HeadOutletStateChangeTracker _head { get; set; } = default!;
        [Inject] private IServiceProvider _services { get; set; } = default!;
        [Inject] private IFileDialog _dialog { get; set; } = default!;

        private Customization _model = default!;
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

        private async Task HandleFilesSelected(IBrowserFile file)
        {
            try
            {
                using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB limit
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                ms.Position = 0;
                var dir = Path.Combine(CyrenaBuilder.AppDataDirectory, "public", "wallpapers");
                if(Directory.Exists(dir))
                    Directory.Delete(dir, true);
                if(!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, file.Name), ms.ToArray());
                _model.Background.BackgroundImage = $"wallpapers/{file.Name}";
                SaveCustoms();
            }catch(Exception ex)
            {
                _snackbar.Add($"{file.Name} Error: {ex.Message}", Severity.Error);
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

        private async Task PickWallpaper()
        {
            var result = await _dialog.OpenAsync("Select Wallpaper", ("Image", [".png", ".jpg", ".jpeg"]));
            if(result != null)
            {
                var fileName = Path.GetFileName(result);
                var data = File.ReadAllBytes(result);
                var dir = Path.Combine(CyrenaBuilder.AppDataDirectory, "public", "wallpapers");
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, fileName), data);
                _model.Background.BackgroundImage = $"wallpapers/{fileName}";
                SaveCustoms();
            }
        }
    }
}
