using Cyrena.Components.Shared;
using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Options;
using MudBlazor;

namespace Cyrena.Services
{
    internal class PinService : IPinService
    {
        private readonly ISettingsService _settings;
        private readonly IDialogService _dialog;
        private readonly ISnackbar _toasts;
        public PinService(ISettingsService settings, IDialogService dialog, ISnackbar toasts)
        {
            _settings = settings;
            _dialog = dialog;
            _toasts = toasts;
        }

        public event EventHandler<bool>? AuthorizationChanged;
        private bool _authorized { get; set; }
        public bool IsAuthorized()
        {
            var options = _settings.Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
            if (!options.UsePin)
                return true;
            if(string.IsNullOrEmpty(options.Pin))
            {
                _toasts.Add("Configure pin in Settings > Application", Severity.Warning);
                return true;
            }
            return _authorized;
        }

        public bool Authorize(string? pin)
        {
            var options = _settings.Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
            if (string.IsNullOrEmpty(options.Pin))
            {
                _toasts.Add("Configure pin in Settings > Application", Severity.Warning);
                return true;
            }
            return pin == options.Pin;
        }

        public async Task<bool> AuthorizeAsync()
        {
            var options = _settings.Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
            if (string.IsNullOrEmpty(options.Pin))
                return Authorize(null);
            var vm = new PinViewModel()
            {
                OldPin = options.Pin
            };
            var reference = await _dialog.ShowAsync<EnterPin>(null,new DialogParameters() { { "Model", vm} }, new DialogOptions()
            {
                MaxWidth = MaxWidth.Small,
                NoHeader = true,
                CloseOnEscapeKey = false,
                CloseOnNavigation = false,
                CloseButton = false
            });
            var result = await reference.Result;
            if(result is { Canceled:false} && result.Data is PinViewModel pin)
                _authorized = Authorize(pin.ConfirmOldPin);
            else
                _authorized = false;
            AuthorizationChanged?.Invoke(this, _authorized);
            return _authorized;
        }

        public async Task ConfigureAsync()
        {
            var options = _settings.Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
            var model = new PinViewModel() { OldPin = options.Pin };
            var reference = await _dialog.ShowAsync<ConfigurePin>("Configure Pin", new DialogParameters()
            {
                {nameof(ConfigurePin.Model), model }
            }, new DialogOptions()
            {
                MaxWidth = MaxWidth.Small,
                FullWidth = true
            });
            var result = await reference.Result;
            if(result is { Canceled:false} && result.Data is PinViewModel pin)
            {
                options.Pin = pin.NewPin;
                _settings.Save(ApplicationOptions.Key, options);
            }
        }
    }
}
