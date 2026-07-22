using Cyrena.Components.Shared;
using Cyrena.Contracts;
using Cyrena.Extensions;
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
            if (string.IsNullOrEmpty(options.PinHash))
            {
                _toasts.Add("Configure pin in Settings > Application", Severity.Warning);
                return true;
            }
            return _authorized;
        }

        public bool HasPin()
        {
            var options = _settings.Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
            return options.UsePin && !string.IsNullOrEmpty(options.PinHash);
        }

        public bool VerifyPin(string? pin)
        {
            var options = _settings.Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
            if (string.IsNullOrEmpty(options.PinHash))
            {
                _toasts.Add("Configure pin in Settings > Application", Severity.Warning);
                return true;
            }
            if (string.IsNullOrEmpty(pin))
                return false;
            return PinHasher.VerifyPin(pin, options.PinHash);
        }

        public async Task<bool> AuthorizeAsync()
        {
            if (!HasPin())
                return true;

            var vm = new PinViewModel();
            var reference = await _dialog.ShowAsync<EnterPin>(null, new DialogParameters<EnterPin>() { { x => x.Model, vm } }, new DialogOptions()
            {
                MaxWidth = MaxWidth.Small,
                NoHeader = true,
                CloseOnEscapeKey = false,
                CloseOnNavigation = false,
                CloseButton = false
            });
            var result = await reference.Result;
            if (result is { Canceled: false } && result.Data is PinViewModel pin)
                _authorized = VerifyPin(pin.ConfirmOldPin);
            else
                _authorized = false;
            AuthorizationChanged?.Invoke(this, _authorized);
            return _authorized;
        }

        public async Task ConfigureAsync()
        {
            var options = _settings.Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
            var model = new PinViewModel();
            var reference = await _dialog.ShowAsync<ConfigurePin>("Configure Pin", new DialogParameters<ConfigurePin>()
            {
                { x => x.Model, model },
                { x => x.HasExistingPin, !string.IsNullOrEmpty(options.PinHash) }
            }, new DialogOptions()
            {
                MaxWidth = MaxWidth.Small,
                FullWidth = true
            });
            var result = await reference.Result;
            if (result is { Canceled: false } && result.Data is PinViewModel pin)
            {
                if (!string.IsNullOrEmpty(pin.NewPin))
                {
                    options.PinHash = PinHasher.HashPin(pin.NewPin);
                    options.UsePin = true;
                }
                _settings.Save(ApplicationOptions.Key, options);
            }
        }
    }
}
