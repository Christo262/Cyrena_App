namespace Cyrena.Contracts
{
    public interface ISettingsService
    {
        void Save<T>(string key, T value) where T : class;
        T? Read<T>(string key) where T : class;
    }
}
