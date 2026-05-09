using Cyrena.Synthesis.Models;

namespace Cyrena.Synthesis.Options
{
    public sealed class SynthesisBuilder
    {
        private readonly List<CapabilityAbiDescriptor> _descriptors;
        public SynthesisBuilder()
        {
            _descriptors = new List<CapabilityAbiDescriptor>();
        }

        public IReadOnlyList<CapabilityAbiDescriptor> CapabilityAbis => _descriptors.AsReadOnly();

        public bool AddDescriptor(CapabilityAbiDescriptor descriptor)
        {
            if(_descriptors.Any(x => x.ServiceType == descriptor.ServiceType))
                return false;
            _descriptors.Add(descriptor);
            return true;
        }
    }
}
