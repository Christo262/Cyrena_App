using Cyrena.Contracts;

namespace Cyrena.Options
{
    public class CodeLanguages
    {
        private readonly Dictionary<string, string> _langs;
        public CodeLanguages()
        {
            _langs = new Dictionary<string, string>()
            {
                {"c", "c" },
                {"cpp", "cpp" },
                {"h", "c" },
                {"hpp", "cpp" },
                {"cs", "csharp" },
                {"razor", "html" }, //?
                {"css", "css" },
                {"js", "javascript" },
                {"md", "markdown" },
                {"csproj", "xml" },
                {"xml", "xml" },
                {"json", "json" },
                {"ino", "cpp" },
            };
        }

        public string GetFileLanguage(string extension)
        {
            extension = extension.Replace(".", "").ToLower();
            if (_langs.TryGetValue(extension, out var lang))
                return lang;
            return "plaintext";
        }
    }
}
