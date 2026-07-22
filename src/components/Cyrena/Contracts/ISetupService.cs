namespace Cyrena.Contracts
{
    public interface ISetupService
    {
        event EventHandler<EventArgs>? OnDefaultConnectionSet;
        Task SetDefaultConnectionId(string connectionId);
        Task<string?> GetDefaultConnection();

        void InvokeDefaultConnectionSet();
    }
}
