using Cyrena.Models;
using System.Text;

namespace Cyrena.Coding.Models
{
    /// <summary>
    /// Easy way to capture standard output from Process and return to AI. Implements <see cref="ISuppressibleResult"/>
    /// </summary>
    public sealed class ConsoleOutput : List<ConsoleLine>, ISuppressibleResult
    {
        private readonly object _lock;
        public ConsoleOutput()
        {
            _lock = new object();
        }

        public string? Command { get; set; }

        public void WriteLine(string level, string? content)
        {
            lock (_lock)
            {
                this.Add(new ConsoleLine()
                {
                    Level = level,
                    Content = content
                });
            }
        }
        
        public string Suppress()
        {
            var levels = this.Select(x => x.Level).Distinct();
            var sb = new StringBuilder();
            sb.Append($"{Command ?? "unknown_cmd"}: ");
            var st = string.Join(", ", levels.Select(x => $"{x}_items={this.Count(t => t.Level == x)}"));
            sb.Append(st);
            return sb.ToString();
        }
    }

    public class ConsoleLine
    {
        public string Level { get; set; } = "info";
        public string? Content { get; set; }
    }
}
