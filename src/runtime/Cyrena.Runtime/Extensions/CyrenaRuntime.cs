using Cyrena.Contracts;
using Cyrena.Options;
using Cyrena.Runtime.Services;

namespace Cyrena.Extensions
{
    public static class CyrenaRuntime
    {
        internal static ISettingsService? _existingSettings;
        public static ISettingsService CreateSettings()
        {
            if (_existingSettings == null)
                _existingSettings = new SettingsService(CyrenaBuilder.AppDataDirectory);
            return _existingSettings;
        }
    }
}
