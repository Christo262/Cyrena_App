namespace Cyrena.Contracts
{
    /// <summary>
    /// For services or actions that need to happen before <see cref="IDeveloperContext"/> is used
    /// </summary>
    public interface IStartupTask
    {
        /// <summary>
        /// Controls order of execution
        /// </summary>
        int Order { get; }
        /// <summary>
        /// Executes the task
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RunAsync(CancellationToken cancellationToken = default);
    }
}
