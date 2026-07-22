using Cyrena.Models;
using System.Text;

namespace Cyrena.Coding.Models
{
    public class DevelopFile : DevelopItem, ISuppressibleResult
    {
        public bool ReadOnly { get; set; } = false;

        public string Suppress()
        {
            return ReadOnly
                ? $"[FILE:{Id}; read-only; content omitted; use Code_read/Code_read_lines]"
                : $"[FILE:{Id}; content omitted; use Code_read/Code_read_lines before editing]";
        }
    }

    public class DevelopFileContent : DevelopFile
    {
        public DevelopFileContent() { }
        public DevelopFileContent(DevelopFile file, string? content)
        {
            Id = file.Id;
            Name = file.Name;
            RelativePath = file.RelativePath;
            Content = content;
            ReadOnly = file.ReadOnly;
        }
        public string? Content { get; set; }
    }

    public class DevelopFileLines : DevelopFile
    {
        public DevelopFileLines()
        {
            Lines = new List<DevelopFileLine>();
        }

        public DevelopFileLines(DevelopFile file, string? content)
        {
            Id = file.Id;
            Name = file.Name;
            RelativePath = file.RelativePath;
            ReadOnly = file.ReadOnly;
            Lines = new List<DevelopFileLine>();

            if (string.IsNullOrEmpty(content))
                return;

            var lines = SplitLines(content);

            for (var i = 0; i < lines.Count; i++)
            {
                Lines.Add(new DevelopFileLine
                {
                    Index = i,
                    Text = lines[i]
                });
            }
        }

        public List<DevelopFileLine> Lines { get; set; }

        public int LineCount => Lines.Count;

        public override string ToString()
        {
            return string.Join("\n", Lines.OrderBy(x => x.Index).Select(x => x.Text ?? string.Empty));
        }

        private static List<string> SplitLines(string content)
        {
            return content
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split('\n', StringSplitOptions.None)
                .ToList();
        }
    }

    public class DevelopFileLine
    {
        public int Index { get; set; }
        public string? Text { get; set; }
    }
}
