using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Developer.Components.Shared;
using Cyrena.Developer.Contracts;
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
    internal class SolutionBuilder : ICodeBuilder
    {
        private readonly IServiceProvider _services;
        private readonly IStore<ProjectModel> _store;
        private readonly IKernelController _kernel;
        public SolutionBuilder(IServiceProvider services, IStore<ProjectModel> store, IKernelController kernel)
        {
            _services = services;
            _store = store;
            _kernel = kernel;
        }

        public string Id => ".net-solution";

        public async Task<DevelopPlan> ConfigureAsync(DevelopOptions options)
        {
            var sln_path = options.ChatConfiguration[DotnetOptions.SolutionFilePath];
            if (string.IsNullOrEmpty(sln_path))
                throw new NullReferenceException($"{DotnetOptions.SolutionFilePath} not set");
            var info = SolutionParser.GetProjectDetails(sln_path);
            if (info.Count == 0)
                throw new InvalidOperationException($"Solution requires at least one supported project");
            var sln_dir = Path.GetDirectoryName(sln_path);
            var project_types = _services.GetServices<IDotnetProjectType>();
            var projects = new List<ProjectModel>(await _store.FindManyAsync(x => x.ConversationId == options.ChatConfiguration.Id));

            foreach(var item in info)
            {
                var project = projects.FirstOrDefault(x => x.ProjectFilePath == item.AbsolutePath);
                var project_type = project_types.FirstOrDefault(x => x.IsOfType(item));
                if (project == null)
                {
                    var fi = new FileInfo(item.AbsolutePath);
                    project = new ProjectModel()
                    {
                        ConversationId = options.ChatConfiguration.Id,
                        ProjectFilePath = item.AbsolutePath,
                        ProjectName = item.ProjectName,
                        ProjectDirectory = fi.DirectoryName!,
                        ProjectTypeId = project_type?.Id,
                        ProjectTypeName = project_type?.ProjectTypeName
                    };
                    await _store.AddAsync(project);
                    projects.Add(project);
                }
                project.ProjectTypeId = project_type?.Id;
                project.ProjectTypeName = project_type?.ProjectTypeName;
                //TODO detect when project is removed
            }
            var sln_model = new SolutionViewModel(options.ChatConfiguration[DevelopOptions.RootDirectory]!);
            projects.ForEach(item =>
            {
                var type = project_types.FirstOrDefault(x => x.Id == item.ProjectTypeId);
                DevelopPlan? proj_plan = null;
                if (type != null)
                    proj_plan = type.IndexPlan(item);
                var proj_model = new ProjectViewModel(item);
                proj_model.Plan = proj_plan;
                sln_model.Projects.Add(proj_model);
            });
            if(sln_model.Projects.Where(x => x.Plan != null).Count() == 0)
                throw new InvalidOperationException($"Solution requires at least one supported project");
            ProjectViewModel active;
            if (string.IsNullOrEmpty(options.ChatConfiguration[DotnetOptions.LastProject]))
                active = sln_model.Projects.FirstOrDefault(x => x.Plan != null)!;
            else
            {
                var act_t = sln_model.Projects.FirstOrDefault(x => x.Id == options.ChatConfiguration[DotnetOptions.LastProject]);
                if(act_t == null || act_t.Plan == null)
                    active = sln_model.Projects.FirstOrDefault(x => x.Plan != null)!;
                else
                    active = act_t;
            }

            options.ChatConfiguration[DotnetOptions.LastProject] = active.Id;
            options.Services.AddSingleton(sln_model);
            options.Services.AddSingleton(project_types);
            options.Services.AddSingleton(_store);
            options.Services.AddSingleton<ISolutionController, SolutionController>();
            options.Plugins.AddFromType<DotnetSolution>();
            options.Plugins.AddFromType<Dotnet>();
            options.Plugins.AddFromType<Blazor>();
            options.Plugins.AddFromType<MVC>();
            options.Plugins.AddFromType<Www>();
            options.KernelBuilder.AddStartupTask<PromptStartupTask>();
            options.AddApiReferencing();
            options.KernelBuilder.AddToolbarComponent<SolutionSelector>(ToolbarAlignment.Start);
            options.Services.AddSingleton<DotnetFileWatcher>();
            return active.Plan!;
        }

        public async Task DeleteAsync(ChatConfiguration config)
        {
            await _store.DeleteManyAsync(x => x.ConversationId == config.Id);
        }

        public async Task EditAsync(ChatConfiguration config, IServiceProvider services)
        {
            var dialog = services.GetRequiredService<DialogService>();
            var rf = await dialog.ShowModal<DotnetConversationForm>(new ResultDialogOption()
            {
                Title = ".NET Solution",
                Size = Size.Medium,
                ComponentParameters = new()
                {
                    {nameof(DotnetConversationForm.Configuration), config }
                },
                ButtonYesText = "Save",
                ButtonNoText = "Cancel",
            });
            if (rf == DialogResult.Yes)
                await _kernel.UpdateAsync(config, true);
        }
    }

    internal class PromptStartupTask : IStartupTask
    {
        private readonly IChatMessageService _chat;
        private readonly DotnetFileWatcher _watcher;
        public PromptStartupTask(IChatMessageService chat, DotnetFileWatcher watcher)
        {
            _chat = chat;
            _watcher = watcher;
        }

        public int Order => 1;

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            _watcher.Start();
            var prompt = ReadDotnetPrompt();
            await _chat.AddSystemMessage(prompt);
        }

        private string ReadDotnetPrompt()
        {
            var assembly = typeof(PromptStartupTask).Assembly;
            var resourceName = "Cyrena.Developer.dotnet-prompt.md";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException(resourceName);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
