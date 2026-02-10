using BlazorMonaco.Editor;
using Microsoft.AspNetCore.Components;
using Cyrena.Models;
using Cyrena.Options;

namespace Cyrena.Components.Shared
{
    public partial class CodeEditor
    {
        [Parameter]
        [EditorRequired]
        public ProjectFile File { get; set; } = default!;

        private CodeLanguages _langs = new();

        private string? _lang { get; set; }
        private string? _text { get; set; }

        protected override void OnInitialized()
        {
            var ext = File.Name.Split('.').LastOrDefault();
            if (ext == null)
                _lang = "plaintext";
            else
                _lang = _langs.GetFileLanguage(ext);
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
