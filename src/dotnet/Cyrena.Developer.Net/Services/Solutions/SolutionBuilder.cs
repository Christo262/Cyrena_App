using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Developer.Components.Shared;
using Cyrena.Developer.Contracts;
using Cyrena.Developer.Models;
using Cyrena.Developer.Options;
using Cyrena.Developer.Plugins;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Developer.Services
{
    internal class SolutionBuilder : ICodeBuilder
    {
        private readonly IServiceProvider _services;
        private readonly IKernelController _kernel;
        public SolutionBuilder(IServiceProvider services, IKernelController kernel)
        {
            _services = services;
            _kernel = kernel;
        }

        public string Id => ".net-solution";

        public async Task<DevelopPlan> ConfigureAsync(CyrenaKernelBuilder options)
        {
            var sln_path = options.ChatConfiguration[DotnetOptions.SolutionFilePath];
            if (string.IsNullOrEmpty(sln_path))
                throw new NullReferenceException($"{DotnetOptions.SolutionFilePath} not set");
            var info = SolutionParser.GetProjectDetails(sln_path);
            if (info.Count == 0)
                throw new InvalidOperationException($"Solution requires at least one supported project");
            var sln_dir = Path.GetDirectoryName(sln_path);
            var project_types = _services.GetServices<IDotnetProjectType>();
            var projects = new List<ProjectModel>();

            foreach(var item in info)
            {
                var project_type = project_types.FirstOrDefault(x => x.IsOfType(item));
                var fi = new FileInfo(item.AbsolutePath);
                var project = new ProjectModel()
                {
                    ConversationId = options.ChatConfiguration.Id,
                    ProjectFilePath = item.AbsolutePath,
                    ProjectName = item.ProjectName,
                    ProjectDirectory = fi.DirectoryName!,
                    ProjectTypeId = project_type?.Id,
                    ProjectTypeName = project_type?.ProjectTypeName,
                    Id = Guid.NewGuid().ToString()
                };
                projects.Add(project);
                if(project_type != null)
                {
                    project.ProjectTypeId = project_type.Id;
                    project.ProjectTypeName = project_type.ProjectTypeName;
                    project_type.IndexPlan(project);
                }
            }
            var sln_model = new SolutionViewModel(options.ChatConfiguration[DevelopOptions.RootDirectory]!);
            sln_model.Projects.AddRange(projects);
            if(sln_model.Projects.Where(x => x.Plan != null).Count() == 0)
                throw new InvalidOperationException($"Solution requires at least one supported project");
            ProjectModel active;
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
            options.Services.AddSingleton<ISolutionController, SolutionController>();
            options.Plugins.AddFromType<DotnetSolution>();
            options.Plugins.AddFromType<Dotnet>();
            options.Plugins.AddFromType<Blazor>();
            options.Plugins.AddFromType<MVC>();
            options.Plugins.AddFromType<Www>();
            var prompt = Resources.Read(typeof(DotnetExtension).Assembly, "Cyrena.Developer.Resources.dotnet-prompt.md");
            options.GetFeatureOption<IPromptManager>().AddPrompt(0, prompt);
            options.KernelBuilder.AddToolbarComponent<SolutionSelector>(ToolbarAlignment.Start);
            options.KernelBuilder.AddToolbarComponent<RefreshPlan>(ToolbarAlignment.Start);
            return active.Plan!;
        }

        public Task DeleteAsync(ChatConfiguration config)
        {
            return Task.CompletedTask;
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
}
