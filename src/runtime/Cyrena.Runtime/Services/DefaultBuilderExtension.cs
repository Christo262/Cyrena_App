using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Cyrena.Contracts;
using Cyrena.Persistence.Contracts;
using Cyrena.Runtime.Models;
using Cyrena.Runtime.Plugins;

namespace Cyrena.Runtime.Services
{
    internal class DefaultBuilderExtension : IDeveloperContextExtension
    {
        private readonly IStore<Note> _notes;
        public DefaultBuilderExtension(IStore<Note> notes)
        {
            _notes = notes;
        }

        public int Priority => 0;

        public Task ExtendAsync(IDeveloperContextBuilder builder)
        {
            builder.Plugins.AddFromType<ProjectPlugin>();
            builder.Plugins.AddFromType<FilesPlugin>();
            builder.Services.AddSingleton<IStore<Note>>(_notes);
            builder.Plugins.AddFromType<NotesPlugin>();
            builder.Plugins.AddFromType<DateTimePlugin>();
            return Task.CompletedTask;
        }
    }
}
