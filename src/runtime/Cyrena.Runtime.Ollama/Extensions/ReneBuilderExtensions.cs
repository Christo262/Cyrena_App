using Microsoft.Extensions.DependencyInjection;
using Cyrena.Contracts;
using Cyrena.Options;
using Cyrena.Runtime.Ollama.Components.Shared;
using Cyrena.Runtime.Ollama.Models;
using Cyrena.Runtime.Ollama.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddOllama(this CyrenaBuilder builder)
        {
            builder.AddScopedStore<OllamaConnectionInfo>("ollama_connections");
            builder.Services.AddScoped<IConnectionProvider, OllamaConnectionProvider>();

            builder.AddSettingsComponent<OllamaSettings>();

            return builder;
        }
    }
}
