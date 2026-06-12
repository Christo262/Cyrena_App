using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.VisualStudio.Contracts;

namespace Cyrena.VisualStudio.Services;

public class CsProjFileHandler : IProjHandler
{
    public string Filter => "csproj";
    public string Title => "C# Project";
    public string PromptId => "Cyrena.VisualStudio.Resources.cs-prompt.md";
    public string Description { get; } = "Build a .NET C# project with custom structure.";

    public void Initialize(DevelopPlan plan)
    {
        plan.AddIgnoredDirectory("bin");
        plan.AddIgnoredDirectory("obj");
        plan.Discover("cs", true, false);
        plan.IndexFiles("csproj", "root_csproj_", true);
            
        if(plan.ContainsFileTypes("razor"))
            plan.Discover("razor", true, false);
        if(plan.ContainsFileTypes("cshtml"))
            plan.Discover("cshtml", true, false);
        if(plan.ContainsFileTypes("json"))
            plan.Discover("json", true, false);
        if(plan.ContainsFileTypes("xaml"))
            plan.Discover("xaml", true, false);
        if (Directory.Exists(Path.Combine(plan.RootDirectory, "wwwroot")))
        {
            var folder = plan.GetOrCreateFolder("wwwroot", "wwwroot");
            plan.Discover(folder, "css",false);
            plan.Discover(folder, "js",false);
            plan.Discover(folder, "html",false);
        }
    }
}