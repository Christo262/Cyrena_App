namespace Cyrena.Contracts
{
    public interface IPinService
    {
        event EventHandler<bool>? AuthorizationChanged;
        bool IsAuthorized();
        bool Authorize(string? pin);
        Task<bool> AuthorizeAsync();
        Task ConfigureAsync();
    }
}
