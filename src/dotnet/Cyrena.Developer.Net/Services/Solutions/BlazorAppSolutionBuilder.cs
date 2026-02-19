using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Developer.Components.Shared;
using Cyrena.Developer.Contracts;
using Cyrena.Developer.Extensions;
using Cyrena.Developer.Models;
using Cyrena.Developer.Options;
using Cyrena.Developer.Plugins;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Developer.Services
{
    internal class BlazorAppSolutionBuilder : ICodeBuilder
    {
        private readonly IServiceProvider _services;
        private readonly IStore<ProjectModel> _store;
        private readonly IKernelController _kernel;
        public BlazorAppSolutionBuilder(IServiceProvider services, IStore<ProjectModel> store, IKernelController kernel)
        {
            _services = services;
            _store = store;
            _kernel = kernel;
        }

        public string Id => DotnetOptions.CsBlazorApp;

        public async Task<DevelopPlan> ConfigureAsync(DevelopOptions options)
        {
            var proj = options.ChatConfiguration[DotnetOptions.ProjectFilePath];
            if (proj == null || !File.Exists(proj))
                throw new NullReferenceException("Project file path not set");
            var csproj = ProjectParser.ParseProject(proj);
            options.ChatConfiguration["namespace"] = csproj.RootNamespace;
            options.ChatConfiguration[DevelopOptions.RootDirectory] = Path.GetDirectoryName(proj);

            var proj_model = await _store.FindAsync(x => x.ConversationId == options.ChatConfiguration.Id);
            if(proj_model == null)
            {
                proj_model = new ProjectModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    ConversationId = options.ChatConfiguration.Id,
                    ProjectFilePath = proj,
                    ProjectName = Path.GetFileName(proj),
                    ProjectDirectory = options.ChatConfiguration[DevelopOptions.RootDirectory]!,
                    ProjectTypeId = Id,
                    ProjectTypeName = "Blazor App"
                };
                await _store.AddAsync(proj_model);
            }    

            var sln_model = new SolutionViewModel(options.ChatConfiguration[DevelopOptions.RootDirectory]!);
            var project = new ProjectViewModel(proj_model);
            sln_model.Projects.Add(project);
            project.Plan = new DevelopPlan(project.ProjectDirectory);
            project.Plan.IndexDefaultCSharpProject();
            project.Plan.IndexBlazorProjectType();
            project[DotnetOptions.CSharp.Namespace] = csproj.RootNamespace;
            project[DotnetOptions.CSharp.TargetFrameworks] = csproj.TargetFrameworks;

            var project_types = _services.GetServices<IDotnetProjectType>();
            options.ChatConfiguration[DotnetOptions.LastProject] = project.Id;
            options.Services.AddSingleton(sln_model);
            options.Services.AddSingleton(project_types);
            options.Services.AddSingleton(_store);
            options.Services.AddSingleton<ISolutionController, SolutionController>();
            options.Plugins.AddFromType<Dotnet>();
            options.Plugins.AddFromType<Blazor>();
            options.Plugins.AddFromType<Www>();
            options.AddApiReferencing();
            options.KernelBuilder.AddStartupTask<BlazorPromptStartupTask>();
            options.Services.AddSingleton<DotnetFileWatcher>();
            return project.Plan;
        }

        public async Task DeleteAsync(ChatConfiguration config)
        {
            await _store.DeleteManyAsync(x => x.ConversationId == config.Id);
        }

        public async Task EditAsync(ChatConfiguration config, IServiceProvider services)
        {
            var dialog = services.GetRequiredService<DialogService>();
            var rf = await dialog.ShowModal<DotnetCsConfig>(new ResultDialogOption()
            {
                Title = "Blazor App",
                Size = Size.Medium,
                ComponentParameters = new()
                {
                    {nameof(DotnetCsConfig.Model), config }
                },
                ButtonYesText = "Save",
                ButtonNoText = "Cancel",
            });
            if (rf == DialogResult.Yes)
                await _kernel.UpdateAsync(config, true);
        }
    }

    internal class BlazorPromptStartupTask : IStartupTask
    {
        private readonly IChatMessageService _chat;
        private readonly DotnetFileWatcher _watcher;
        public BlazorPromptStartupTask(IChatMessageService chat, DotnetFileWatcher watcher)
        {
            _chat = chat;
            _watcher = watcher;
        }

        public int Order => 1;

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            _watcher.Start();
            var prompt = ReadBlazorAppPrompt();
            await _chat.AddSystemMessage(prompt);
        }

        private string ReadBlazorAppPrompt()
        {
            var assembly = typeof(PromptStartupTask).Assembly;
            var resourceName = "Cyrena.Developer.blazor-app-prompt.md";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException(resourceName);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
