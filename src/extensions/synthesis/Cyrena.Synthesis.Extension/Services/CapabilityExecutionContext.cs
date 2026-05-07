using Cyrena.Synthesis.Contracts;
using Cyrena.Synthesis.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Synthesis.Services
{
    internal class CapabilityExecutionContext : ICapabilityExecutionContext
    {
        private readonly IEnumerable<CapabilityAbiDescriptor> _descriptors;
        private readonly IServiceProvider _services;
        public CapabilityExecutionContext(
            IServiceProvider services, 
            ICapabilityArgs args, 
            ICapabilityLogger logger, 
            IEnumerable<CapabilityAbiDescriptor> descriptors)
        {
            _services = services;
            Args = args;
            Log = logger;
            _descriptors = descriptors;
            Result = new CapabilityResultWriter();
        }

        public ICapabilityArgs Args { get; }
        public ICapabilityLogger Log { get; }

        public ICapabilityResultWriter Result { get; }

        public T GetRequiredService<T>()
            where T : class 
        {
            if(typeof(T) == typeof(ICapabilityArgs))
                return (T)Args;
            if (typeof(T) == typeof(ICapabilityLogger))
                return (T)Log;
            if (typeof(T) == typeof(ICapabilityResultWriter))
                return (T)Result;
            var srv = _services.GetRequiredService<T>();
            if(_descriptors.Any(x => x.ServiceType == typeof(T)))
                return srv;
            throw new InvalidOperationException($"{typeof(T)} is not a valid service");
        }

        public T? GetService<T>()
            where T : class
        {
            if (typeof(T) == typeof(ICapabilityArgs))
                return (T)Args;
            if (typeof(T) == typeof(ICapabilityLogger))
                return (T)Log;
            if (typeof(T) == typeof(ICapabilityResultWriter))
                return (T)Result;
            var srv = _services.GetService<T>();
            if (_descriptors.Any(x => x.ServiceType == typeof(T)))
                return srv;
            return null;
        }
    }
}
