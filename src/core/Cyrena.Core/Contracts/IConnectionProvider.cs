using Cyrena.Models;
using Microsoft.SemanticKernel;

namespace Cyrena.Contracts
{
    public interface IConnectionProvider
    {
        Task<IEnumerable<ConnectionInfo>> ListConnectionsAsync();
        Task<bool> HasConnectionAsync(string id);
        Task AttachAsync(IKernelBuilder builder, string connectionId);
    }
}
