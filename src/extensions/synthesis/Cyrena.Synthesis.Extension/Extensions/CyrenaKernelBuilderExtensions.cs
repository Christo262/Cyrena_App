using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Synthesis.Contracts;
using Cyrena.Synthesis.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Synthesis.Extensions
{
    public static class CyrenaKernelBuilderExtensions
    {
        public static CyrenaKernelBuilder ConfigureDefaultAbis(this CyrenaKernelBuilder builder)
        {
            builder.AddCapabilityAbiDescriptor<ICapabilityExecutionContext>(Resources.Read(typeof(FileSystemAbi).Assembly, "Cyrena.Synthesis.Resources.abi-exe-context.md"));
            builder.AddCapabilityAbiDescriptor<ICapabilityArgs>(Resources.Read(typeof(FileSystemAbi).Assembly, "Cyrena.Synthesis.Resources.abi-args.md"));
            builder.AddCapabilityAbiDescriptor<ICapabilityLogger>(Resources.Read(typeof(FileSystemAbi).Assembly, "Cyrena.Synthesis.Resources.abi-logging.md"));
            builder.AddCapabilityAbiDescriptor<ICapabilityResultWriter>(Resources.Read(typeof(FileSystemAbi).Assembly, "Cyrena.Synthesis.Resources.abi-results.md"));

            builder.Services.AddSingleton<IFileSystemAbi, FileSystemAbi>();
            builder.AddCapabilityAbiDescriptor<IFileSystemAbi>(Resources.Read(typeof(FileSystemAbi).Assembly, "Cyrena.Synthesis.Resources.abi-file-system.md"));
            return builder;
        }
    }
}
