using Microsoft.SemanticKernel;

namespace Cyrena.Contracts
{
    /// <summary>
    /// Kernel Locked
    /// </summary>
    public interface IKernelResolver
    {
        Func<Kernel> Resolve { get; }
    }
}
