namespace Cyrena.Ollama.Web.Options
{
    public class OllamaWebOptions
    {
        public const string Key = "ollama.web";

        public string? APIKey { get; set; }
        public bool Enabled { get; set; } = true;
    }
}
