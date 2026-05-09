namespace Cyrena.Synthesis.Models
{
    /// <summary>
    /// A single log entry captured during dynamic capability execution.
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string ScriptId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
