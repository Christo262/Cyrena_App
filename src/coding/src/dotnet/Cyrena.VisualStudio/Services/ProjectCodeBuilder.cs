using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Contracts;
using Cyrena.Dotnet.Extensions;
using Cyrena.Dotnet.Models;
using Cyrena.Dotnet.Options;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.VisualStudio.Components.Shared;
using Cyrena.VisualStudio.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using MudBlazor;

namespace Cyrena.VisualStudio.Services;

public class ProjectCodeBuilder : ICodeBuilder
{
    private readonly IKernelController _kernel;
    private readonly IProjHandler _handler;
    public ProjectCodeBuilder(IKernelController kernel, IProjHandler handler)
    {
        _kernel = kernel;
        _handler = handler;
        Id = $"visual.studio.{handler.Filter}";
    }
    
    public string Id { get; }
    public Task<DevelopPlan> ConfigureAsync(CyrenaKernelBuilder options)
    {
        var proj = options.ChatConfiguration[DotnetOptions.ProjectFilePath];
        if (proj == null || !File.Exists(proj))
            throw new NullReferenceException("Project file path not set");
        var vsproj = ProjectParser.ParseProject(proj);
        options.ChatConfiguration["namespace"] = vsproj.RootNamespace;
        options.ChatConfiguration.WorkingDirectory = Path.GetDirectoryName(proj);
        
        var project = new ProjectModel()
        {
            Id = vsproj.FileName,
            ConversationId = options.ChatConfiguration.Id,
            ProjectFilePath = proj,
            ProjectName = Path.GetFileName(proj),
            ProjectDirectory = options.ChatConfiguration.WorkingDirectory!,
            ProjectTypeId = Id,
            ProjectTypeName = $".{_handler.Filter}"
        };
        project[DotnetOptions.Namespace] = vsproj.RootNamespace;
        project[DotnetOptions.TargetFrameworks] = vsproj.TargetFrameworks;
        project.Plan = new DevelopPlan(options.ChatConfiguration.WorkingDirectory!, project.Id);
        var sln_model = new SolutionViewModel(options.ChatConfiguration.WorkingDirectory!);
        sln_model.Projects.Add(project);
        options.UseDynamicDiscovery<DynamicPlanInitializer>();
        options.ChatConfiguration[DotnetOptions.LastProject] = project.Id;
        options.Services.AddSingleton(sln_model);
        options.Services.AddSingleton<IEnumerable<IProjHandler>>([_handler]);
        options.AddDynamicSolutionController();
        var prompt = Resources.Read(typeof(ProjectCodeBuilder).Assembly, _handler.PromptId);
        options.GetFeatureOption<IPromptManager>().AddPrompt(0, prompt);

        if (_handler.Tools.Dotnet)
            options.Plugins.AddFromType<DotnetFunctions>("dotnet");
        if (_handler.Tools.FSharp)
            options.Plugins.AddFromType<FSharpFunctions>("fsproj");

        var plan = new DevelopPlan(options.ChatConfiguration.WorkingDirectory!, project.ProjectName);
        return Task.FromResult(plan);
    }

    public Task DeleteAsync(ChatConfiguration config)
    {
        return Task.CompletedTask;
    }

    public async Task EditAsync(ChatConfiguration config, IServiceProvider services)
    {
        var dialog = services.GetRequiredService<IDialogService>();
        var parameters = new DialogParameters<Configure>
        {
            { nameof(Configure.Model), config },
            {nameof(Configure.Filter), new[]{_handler.Filter} },
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
        var rf = await dialog.ShowAsync<Configure>(_handler.Title, parameters, options);
        var result = await rf.Result;
        if (result is { Canceled: false })
            await _kernel.UpdateAsync(config, true);
    }
}