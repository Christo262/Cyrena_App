namespace Cyrena.LTM.Options
{
    /// <summary>
    /// Configuration options for automatic memory context injection into the AI's prompt.
    /// </summary>
    public class MemoryContextOptions
    {
        public const string Key = "ltm";
        /// <summary>
        /// Whether memory context injection is enabled. Default: true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Maximum number of memories to inject per iteration. Default: 5.
        /// </summary>
        public int MaxMemoriesToInject { get; set; } = 5;

        /// <summary>
        /// Minimum relevance score (0-1) for a memory to be injected. Default: 0.3.
        /// </summary>
        public double MinRelevanceThreshold { get; set; } = 0.3;

        /// <summary>
        /// Maximum age in days for a memory to be considered for injection,
        /// regardless of decay settings. Null means no limit. Default: null.
        /// </summary>
        public int? MaxAgeDays { get; set; }

        /// <summary>
        /// Whether to include fact details in the injected context. Default: true.
        /// </summary>
        public bool IncludeFacts { get; set; } = true;

        /// <summary>
        /// Maximum facts per memory to include in context. Default: 3.
        /// </summary>
        public int MaxFactsPerMemory { get; set; } = 3;
    }
}
