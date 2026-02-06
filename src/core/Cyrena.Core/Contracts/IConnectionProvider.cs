using Microsoft.SemanticKernel;
using Cyrena.Models;

namespace Cyrena.Contracts
{
    public interface IConnectionProvider
    {
        Task<IEnumerable<ConnectionInfo>> ListConnectionsAsync();
        Task<bool> HasConnectionAsync(string id);
        Task<IConnection> CreateAsync(IKernelBuilder builder, string connectionId);
    }
}
