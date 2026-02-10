namespace Cyrena.Contracts
{
    public interface IProjectLoader
    {
        Task<IDeveloperContext> LoadProjectAsync(string projectId, Action<IDeveloperContextBuilder> options);
    }
}
