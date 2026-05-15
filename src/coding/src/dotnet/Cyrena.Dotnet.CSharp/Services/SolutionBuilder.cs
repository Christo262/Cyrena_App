using BootstrapBlazor.Components;
using Cyrena.Coding.Contracts;
using Cyrena.Coding.Models;
using Cyrena.Coding.Options;
using Cyrena.Contracts;
using Cyrena.Dotnet.CSharp.Components.Shared;
using Cyrena.Dotnet.Contracts;
using Cyrena.Dotnet.Models;
using Cyrena.Dotnet.Options;
using Cyrena.Dotnet.CSharp.Plugins;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Cyrena.Dotnet.Extensions;
using Cyrena.Persistence.Options;

namespace Cyrena.Dotnet.CSharp.Services
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
                    Id = $"{fi.DirectoryName?.Replace(sln_dir ??"","").Replace("\\", "_")}_{item.ProjectName}"
                };
                projects.Add(project);
                if(project_type != null)
                {
                    project.ProjectTypeId = project_type.Id;
                    project.ProjectTypeName = project_type.ProjectTypeName;
                    //project_type.IndexPlan(project); //Index later on in DevelopPlanIndexer, no need to do it here
                }
            }
            var sln_model = new SolutionViewModel(options.ChatConfiguration.WorkingDirectory!);
            sln_model.Projects.AddRange(projects);
            if (sln_model.Projects.Count == 0)
                throw new InvalidOperationException($"Solution requires at least one supported project");
            ProjectModel active;
            if (string.IsNullOrEmpty(options.ChatConfiguration[DotnetOptions.LastProject]))
                active = sln_model.Projects.OrderBy(x => x.Plan != null).FirstOrDefault()!;
            else
            {
                var act_t = sln_model.Projects.FirstOrDefault(x => x.Id == options.ChatConfiguration[DotnetOptions.LastProject]);
                if(act_t == null)
                    active = sln_model.Projects.OrderBy(x => x.Plan != null).FirstOrDefault()!;
                else
                    active = act_t;
            }

            options.ChatConfiguration[DotnetOptions.LastProject] = active.Id;
            var persistence = options.GetFeatureOption<ICyrenaPersistenceBuilder>();
            options.Services.AddSingleton(sln_model);
            options.Services.AddSingleton(project_types);
            options.AddSolutionControllerWithProjectOverride();
            options.Plugins.AddFromType<DotnetSolution>();
            options.Plugins.AddFromType<DotnetTools>();
            options.Plugins.AddFromType<Blazor>();
            options.Plugins.AddFromType<MVC>();
            options.Plugins.AddFromType<Www>();
            var prompt = Resources.Read(typeof(DotnetExtension).Assembly, "Cyrena.Dotnet.CSharp.Resources.dotnet-prompt.md");
            options.GetFeatureOption<IPromptManager>().AddPrompt(0, prompt);
            options.AddToolbarComponent<SolutionSelector>(ToolbarAlignment.Start);
            options.Services.AddSingleton<IDevelopPlanIndexer, DevelopPlanIndexer>();
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
