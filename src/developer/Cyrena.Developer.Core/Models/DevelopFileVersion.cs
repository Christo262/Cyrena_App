namespace Cyrena.Developer.Models
{
    /// <summary>
    /// Represents a single versioned snapshot of a file's content.
    /// </summary>
    public class DevelopFileVersion
    {
        public DevelopFileVersion(DevelopFileContent file, string? label = null)
        {
            File = file;
            Timestamp = DateTimeOffset.UtcNow;
            Label = label;
        }

        /// <summary>The snapshot of the file at the time of backup.</summary>
        public DevelopFileContent File { get; }

        /// <summary>UTC time this version was recorded.</summary>
        public DateTimeOffset Timestamp { get; }

        /// <summary>Optional human-readable label (e.g. "before refactor").</summary>
        public string? Label { get; }

        public override string ToString()
            => $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {File.Name}{(Label != null ? $" — {Label}" : string.Empty)}";
    }
}
