using BlazorMonaco.Editor;
using Microsoft.AspNetCore.Components;
using Cyrena.Models;

namespace Cyrena.Components.Shared
{
    public partial class CodeEditor
    {
        [Parameter]
        [EditorRequired]
        public ProjectFile File { get; set; } = default!;

        private string? _lang { get; set; }
        private string? _text { get; set; }

        protected override void OnInitialized()
        {
            if (File.Name.EndsWith(".cs"))
                _lang = "csharp";
            else if (File.Name.EndsWith(".json"))
                _lang = "json";
            else if (File.Name.EndsWith(".razor"))
                _lang = "razor";
            else if (File.Name.EndsWith(".md"))
                _lang = "markdown";
            else
                _lang = "plaintext";
            _text = System.IO.File.ReadAllText(Path.Combine(Context.Project.RootDirectory, File.RelativePath));
        }

        private StandaloneEditorConstructionOptions EditorConstructionOptions(StandaloneCodeEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Language = _lang,
                Value = _text,
                Theme = "vs-dark"
            };
        }
    }
}
