using Cyrena.Synthesis.Models;

namespace Cyrena.Synthesis.Contracts
{
    public interface ICapabilityContext
    {
        DynamicCapability? Current { get; }

        void SetCurrent(DynamicCapability? capability);
        Task<bool> RequestPermissionAsync(DynamicCapability capability, CapabiliyPermissionDescriptor dsc);
        Task<bool> HasPermissionAsync(DynamicCapability capability, string permission);
        Task<bool> HasPermissionAsync(DynamicCapability capability, CapabiliyPermissionDescriptor dsc);
    }
}
