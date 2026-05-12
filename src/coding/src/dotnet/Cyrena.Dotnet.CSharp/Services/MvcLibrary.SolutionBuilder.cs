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

namespace Cyrena.Dotnet.CSharp.Services
{
    internal class MvcLibrarySolutionBuilder : ICodeBuilder
    {
        private readonly IServiceProvider _services;
        private readonly IKernelController _kernel;
        public MvcLibrarySolutionBuilder(IServiceProvider services, IKernelController kernel)
        {
            _services = services;
            _kernel = kernel;
        }

        public string Id => MvcLibrary.Id;

        public async Task<DevelopPlan> ConfigureAsync(CyrenaKernelBuilder options)
        {
            var proj = options.ChatConfiguration[DotnetOptions.ProjectFilePath];
            if (proj == null || !File.Exists(proj))
                throw new NullReferenceException("Project file path not set");
            var csproj = ProjectParser.ParseProject(proj);
            options.ChatConfiguration["namespace"] = csproj.RootNamespace;
            options.ChatConfiguration.WorkingDirectory = Path.GetDirectoryName(proj);

            var project = new ProjectModel()
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = options.ChatConfiguration.Id,
                ProjectFilePath = proj,
                ProjectName = Path.GetFileName(proj),
                ProjectDirectory = options.ChatConfiguration.WorkingDirectory!,
                ProjectTypeId = Id,
                ProjectTypeName = MvcLibrary.Name
            };

            var sln_model = new SolutionViewModel(options.ChatConfiguration.WorkingDirectory!);
            sln_model.Projects.Add(project);
            var idxer = new MvcLibraryProjectType();
            var plan = idxer.IndexPlan(project);
            var project_types = _services.GetServices<IDotnetProjectType>();
            options.ChatConfiguration[DotnetOptions.LastProject] = project.Id;
            options.Services.AddSingleton(sln_model);
            options.Services.AddSingleton(project_types);
            options.AddSolutionController();
            options.Plugins.AddFromType<DotnetTools>();
            options.Plugins.AddFromType<MVC>();
            options.Plugins.AddFromType<Www>();
            var prompt = Resources.Read(typeof(DotnetExtension).Assembly, "Cyrena.Dotnet.CSharp.Resources.mvc-lib-prompt.md");
            options.GetFeatureOption<IPromptManager>().AddPrompt(0, prompt);
            options.Services.AddSingleton<IDevelopPlanIndexer, DevelopPlanIndexer>();
            return plan;
        }

        public Task DeleteAsync(ChatConfiguration config)
        {
            return Task.CompletedTask;
        }

        public async Task EditAsync(ChatConfiguration config, IServiceProvider services)
        {
            var dialog = services.GetRequiredService<DialogService>();
            var rf = await dialog.ShowModal<DotnetCsConfig>(new ResultDialogOption()
            {
                Title = MvcLibrary.Name,
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
}
