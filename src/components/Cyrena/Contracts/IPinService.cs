namespace Cyrena.Contracts
{
    public interface IPinService
    {
        event EventHandler<bool>? AuthorizationChanged;
        bool IsAuthorized();
        bool HasPin();
        bool VerifyPin(string? pin);
        Task<bool> AuthorizeAsync();
        Task ConfigureAsync();
    }
}
