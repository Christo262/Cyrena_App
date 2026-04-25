using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Extensions
{
    public static class CyrenaKernelBuilderExtensions
    {
        public static void AddToolbarComponent<TComponent>(this CyrenaKernelBuilder builder, ToolbarAlignment alignment)
            where TComponent : KernelComponentBase
        {
            builder.Services.AddSingleton<IToolbarComponent>(new ToolbarComponent(typeof(TComponent), alignment));
        }

        [Obsolete]
        public static void AddToolbarComponent<TComponent>(this IKernelBuilder builder, ToolbarAlignment alignment)
            where TComponent : KernelComponentBase
        {
            builder.Services.AddSingleton<IToolbarComponent>(new ToolbarComponent(typeof(TComponent), alignment));
        }
    }
}
