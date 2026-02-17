using Cyrena.Models;

namespace Cyrena.Contracts
{
    public interface IChatConfigurationService
    {
        ChatConfiguration Config { get; }

        Task SaveConfigurationAsync();
    }
}
