using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.VisualStudio.Contracts;
using Cyrena.VisualStudio.Models;

namespace Cyrena.VisualStudio.Services
{
    internal class FsProjHandler : IProjHandler
    {
        public string Filter => "fsproj";
        public string Title => "F# Project";
        public string PromptId => "Cyrena.VisualStudio.Resources.fs-prompt.md";
        public string Description { get; } = "Build a .NET F# project with custom structure.";

        public Tools Tools => new() { Dotnet = true, FSharp = true };

        public void Initialize(DevelopPlan plan)
        {
            plan.AddIgnoredDirectory("bin");
            plan.AddIgnoredDirectory("obj");
            plan.Discover("fs", true, false);

            if (plan.ContainsFileTypes("cshtml"))
                plan.Discover("cshtml", true, false);
            if (plan.ContainsFileTypes("json"))
                plan.Discover("json", true, false);
            if (plan.ContainsFileTypes("xaml"))
                plan.Discover("xaml", true, false);
            if(plan.ContainsFileTypes("axaml"))
                plan.Discover("axaml", true, false);

            if (Directory.Exists(Path.Combine(plan.RootDirectory, "wwwroot")))
            {
                var folder = plan.GetOrCreateFolder("wwwroot", "wwwroot");
                plan.Discover(folder, "css", false);
                plan.Discover(folder, "js", false);
                plan.Discover(folder, "html", false);
            }
        }
    }
}
