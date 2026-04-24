using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Developer.Components.Shared;
using Cyrena.Developer.Contracts;
using Cyrena.Developer.Extensions;
using Cyrena.Developer.Models;
using Cyrena.Developer.Options;
using Cyrena.Developer.Plugins;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Cyrena.Extensions;

namespace Cyrena.Developer.Services
{
    internal class ClassLibrarySolutionBuilder : ICodeBuilder
    {
        private readonly IServiceProvider _services;
        private readonly IKernelController _kernel;
        public ClassLibrarySolutionBuilder(IServiceProvider services, IKernelController kernel)
        {
            _services = services;
            _kernel = kernel;
        }

        public string Id => DotnetOptions.CsClassLibrary;

        public async Task<DevelopPlan> ConfigureAsync(CyrenaKernelBuilder options)
        {
            var proj = options.ChatConfiguration[DotnetOptions.ProjectFilePath];
            if (proj == null || !File.Exists(proj))
                throw new NullReferenceException("Project file path not set");
            var csproj = ProjectParser.ParseProject(proj);
            options.ChatConfiguration["namespace"] = csproj.RootNamespace;
            options.ChatConfiguration[DevelopOptions.RootDirectory] = Path.GetDirectoryName(proj);

            var project = new ProjectModel()
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = options.ChatConfiguration.Id,
                ProjectFilePath = proj,
                ProjectName = Path.GetFileName(proj),
                ProjectDirectory = options.ChatConfiguration[DevelopOptions.RootDirectory]!,
                ProjectTypeId = Id,
                ProjectTypeName = "Class Library"
            };

            var sln_model = new SolutionViewModel(options.ChatConfiguration[DevelopOptions.RootDirectory]!);
            sln_model.Projects.Add(project);
            var idxer = new CSharpClassLibraryProjectType();
            var plan = idxer.IndexPlan(project);

            var project_types = _services.GetServices<IDotnetProjectType>();
            options.ChatConfiguration[DotnetOptions.LastProject] = project.Id;
            options.Services.AddSingleton(sln_model);
            options.Services.AddSingleton(project_types);
            options.Services.AddSingleton<ISolutionController, SolutionController>();
            options.Plugins.AddFromType<Dotnet>();
            var prompt = Resources.Read(typeof(DotnetExtension).Assembly, "Cyrena.Developer.Resources.class-library-prompt.md");
            options.GetFeatureOption<IPromptManager>().AddPrompt(0, prompt);
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
                Title = "Class Library",
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
