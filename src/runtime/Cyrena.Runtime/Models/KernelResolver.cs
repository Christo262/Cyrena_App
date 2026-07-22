using Cyrena.Contracts;
using Microsoft.SemanticKernel;

namespace Cyrena.Runtime.Models
{
    internal class KernelResolver : IKernelResolver
    {
        public KernelResolver(string id, Func<Kernel> resolve)
        {
            Id = id;
            Resolve = resolve;
        }
        public string Id { get; }
        public Func<Kernel> Resolve { get; }
    }
}
