using Cyrena.Contracts;
using Cyrena.Options;
using System;
using System.Threading.Tasks;

namespace Cyrena.Android.Services
{
    internal class SetupService : ISetupService
    {
        private readonly ISettingsService _settings;
        public SetupService(ISettingsService settings)
        {
            _settings = settings;
        }

        public event EventHandler<EventArgs>? OnDefaultConnectionSet;

        public Task<string?> GetDefaultConnection()
        {
            var model = _settings.Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
            return Task.FromResult(model.DefaultConnectionId);
        }

        public void InvokeDefaultConnectionSet()
        {
            OnDefaultConnectionSet?.Invoke(this, EventArgs.Empty);
        }

        public Task SetDefaultConnectionId(string connectionId)
        {
            var model = _settings.Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
            model.DefaultConnectionId = connectionId;
            _settings.Save(ApplicationOptions.Key, model);
            OnDefaultConnectionSet?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }
    }
}
