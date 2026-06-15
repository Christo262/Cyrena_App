using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.VisualStudio.Contracts;
using Cyrena.VisualStudio.Models;

namespace Cyrena.VisualStudio.Services
{
    internal class EsProjHandler : IProjHandler
    {
        public string Filter => "esproj";
        public string Title => "Angular Project";
        public string PromptId => "Cyrena.VisualStudio.Resources.es-prompt.md";
        public string Description { get; } = "Build a Angular project with custom structure.";
        public Tools Tools => new() { Dotnet = true };

        public void Initialize(DevelopPlan plan)
        {
            plan.AddIgnoredDirectory("node_modules");
            plan.AddIgnoredDirectory("bin");
            plan.AddIgnoredDirectory("obj");

            plan.Discover("ts", true, false);
            plan.Discover("js", true, false);
            plan.Discover("json", true, false);
            plan.Discover("md", true, false);
            plan.Discover("html", true, false);
            plan.Discover("css", true, false);
        }
    }
}
