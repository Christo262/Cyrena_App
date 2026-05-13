using Cyrena.Attributes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using System.Reflection;

namespace Cyrena.Models
{
    public abstract class KernelComponentBase : ComponentBase
    {
        [Parameter]
        [EditorRequired]
#pragma warning disable BL0007 // Intentional: Kernel must be processed before OnInitialized.
        public Kernel Kernel
        {
            get
            {
                return _kernel;
            }
            set
            {
                _kernel = value;
                OnKernelSet();
            }
        }
#pragma warning restore BL0007
        private Kernel _kernel { get; set; } = default!;

        private void OnKernelSet()
        {
            var type = GetType();

            var props = type.GetProperties(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

            foreach (var prop in props)
            {
                var att = prop.GetCustomAttribute<KernelInjectAttribute>(inherit: true);
                if (att == null)
                    continue;
                var setter = prop.SetMethod;
                if (setter == null)
                    throw new InvalidOperationException($"{prop.Name} has no setter.");
                object? service;
                if (att.Key == null)
                {
                    service = Kernel.Services.GetService(prop.PropertyType);
                    if (service == null)
                        throw new InvalidOperationException($"Unable to find service of type {prop.PropertyType} in Kernel");
                }
                else
                {
                    service = Kernel.Services.GetKeyedService(prop.PropertyType, att.Key);
                    if (service == null)
                        throw new InvalidOperationException($"Unable to find keyed service of type {prop.PropertyType} with key '{att.Key}' in Kernel");
                }

                setter.Invoke(this, [service]);
            }
        }
    }
}
