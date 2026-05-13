using Cyrena.Contracts;
using Cyrena.HUD.Options;

namespace Cyrena.HUD.Services
{
    internal class SetupService : ISetupService
    {
        private readonly ISettingsService _settings;
        public SetupService(ISettingsService settings)
        {
            _settings = settings;
        }

        public event EventHandler<EventArgs>? OnDefaultConnectionSet;

        public void InvokeDefaultConnectionSet()
        {
            OnDefaultConnectionSet?.Invoke(this, EventArgs.Empty);
        }

        public Task<string?> GetDefaultConnection()
        {
            var model = _settings.Read<WindowOptions>(WindowOptions.Key) ?? new WindowOptions();
            return Task.FromResult(model.DefaultConnectionId);
        }

        public Task SetDefaultConnectionId(string connectionId)
        {
            var model = _settings.Read<WindowOptions>(WindowOptions.Key) ?? new WindowOptions();
            model.DefaultConnectionId = connectionId;
            _settings.Save(WindowOptions.Key, model);
            OnDefaultConnectionSet?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }
    }
}
