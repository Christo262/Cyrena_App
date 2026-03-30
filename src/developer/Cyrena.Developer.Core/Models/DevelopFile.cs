using System.Text;

namespace Cyrena.Developer.Models
{
    public class DevelopFile : DevelopItem
    {
        public bool ReadOnly { get; set; } = false;
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
            Lines = new Dictionary<int, string>();
        }

        public DevelopFileLines(DevelopFile file, string? content)
        {
            Id = file.Id;
            Name = file.Name;
            RelativePath = file.RelativePath;
            ReadOnly = file.ReadOnly;
            Lines = new Dictionary<int, string>();

            if (!string.IsNullOrEmpty(content))
            {
                // Properly split lines while preserving empty lines
                string[] lines;
                if (content.Contains("\r\n"))
                    lines = content.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                else if (content.Contains("\n"))
                    lines = content.Split('\n');
                else
                    lines = content.Split('\r');

                for (int i = 0; i < lines.Length; i++)
                {
                    Lines[i] = lines[i];
                }
            }
        }

        public Dictionary<int, string> Lines { get; set; }

        public override string ToString()
        {
            // Reconstruct with Windows-style line endings (\r\n)
            return string.Join("\r\n", Lines.OrderBy(x => x.Key).Select(x => x.Value));
        }
    }
}
