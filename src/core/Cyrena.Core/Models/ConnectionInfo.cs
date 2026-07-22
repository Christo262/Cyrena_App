using Cyrena.Contracts;

namespace Cyrena.Models
{
    /// <summary>
    /// Provides enough information from a <see cref="IConnectionProvider"/> to display and create a <see cref="IConnection"/>
    /// </summary>
    /// <param name="Id">Unique ID</param>
    /// <param name="Name">Display Name</param>
    /// <param name="Source">Ollama or OpenAI or others if supported</param>
    /// <param name="ModelId">Id of the model</param>
    /// <param name="Provider">Service Reference</param>
    /// <param name="SupportImages">If model supports images</param>
    /// <param name="SupportFiles">If model supports files</param>
    public record ConnectionInfo(string Id, string Name, string Source, string ModelId, IConnectionProvider Provider, bool SupportImages, bool SupportFiles);
}
