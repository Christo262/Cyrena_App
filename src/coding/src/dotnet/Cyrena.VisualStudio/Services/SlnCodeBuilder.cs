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

internal class SlnCodeBuilder(IServiceProvider services, IKernelController kernel) : ICodeBuilder
{
    private readonly IEnumerable<IProjHandler> _handlers = services.GetServices<IProjHandler>();
    
    public string Id => "visual.studio.solution";
    
    public Task<DevelopPlan> ConfigureAsync(CyrenaKernelBuilder options)
    {
        var sln_path = options.ChatConfiguration[DotnetOptions.SolutionFilePath];
        if (string.IsNullOrEmpty(sln_path))
            throw new NullReferenceException($"{DotnetOptions.SolutionFilePath} not set");
        var info = SolutionParser.GetProjectDetails(sln_path);
        if (info.Count == 0)
            throw new InvalidOperationException($"Solution requires at least one supported project");
        var projects = new List<ProjectModel>();
        bool dotnet = false;
        bool fsharp = false;
        foreach (var item in info)
        {
            var vsproj = ProjectParser.ParseProject(item.AbsolutePath);
            var fi = new FileInfo(item.AbsolutePath);
            var ext = fi.Extension.TrimStart('.');
            var handler = _handlers.FirstOrDefault(x => string.Equals(x.Filter, ext, StringComparison.OrdinalIgnoreCase));
            if (handler == null)
                continue;
            if (handler.Tools.Dotnet) dotnet = true;
            if(handler.Tools.FSharp) fsharp = true;
            var project = new ProjectModel()
            {
                ConversationId = options.ChatConfiguration.Id,
                ProjectFilePath = item.AbsolutePath,
                ProjectName = item.ProjectName,
                ProjectDirectory = fi.DirectoryName!,
                ProjectTypeId = $"visual.studio.{ext}",
                ProjectTypeName = fi.Extension,
                Id = vsproj.FileName,
            };
            projects.Add(project);
            project[DotnetOptions.Namespace] = vsproj.RootNamespace;
            project[DotnetOptions.TargetFrameworks] = vsproj.TargetFrameworks;
            project.Plan = new DevelopPlan(project.ProjectDirectory, vsproj.FileName);
            // handler.Initialize(project.Plan);
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
            if (act_t == null)
                active = sln_model.Projects.OrderBy(x => x.Plan != null).FirstOrDefault()!;
            else
                active = act_t;
        }
        options.ChatConfiguration[DotnetOptions.LastProject] = active.Id;
        options.Services.AddSingleton(sln_model);
        options.Services.AddSingleton(_handlers);
        options.UseDynamicDiscovery<DynamicPlanInitializer>();
        options.AddDynamicSolutionController();
        var prompt = Resources.Read(typeof(ProjectCodeBuilder).Assembly, "Cyrena.VisualStudio.Resources.sln-prompt.md");
        options.GetFeatureOption<IPromptManager>().AddPrompt(0, prompt);
        
        if (dotnet)
            options.Plugins.AddFromType<DotnetFunctions>("dotnet");
        if (fsharp)
            options.Plugins.AddFromType<FSharpFunctions>("fsproj");
        options.Plugins.AddFromType<SolutionFunctions>("sln");
        
        options.AddToolbarComponent<SolutionSelector>(ToolbarAlignment.Start);
        
        var plan = new DevelopPlan(options.ChatConfiguration.WorkingDirectory!, active.ProjectName!);
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
            {nameof(Configure.Filter), new[]{".sln", ".slnx"}},
            {nameof(Configure.IsSolutionFile), true}
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
        var rf = await dialog.ShowAsync<Configure>("Solution", parameters, options);
        var result = await rf.Result;
        if (result is { Canceled: false })
            await kernel.UpdateAsync(config, true);
    }
}