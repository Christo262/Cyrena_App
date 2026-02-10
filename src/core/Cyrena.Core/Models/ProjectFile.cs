using System.Text;

namespace Cyrena.Models
{
    public class ProjectFile : ProjectItem
    {
        public bool ReadOnly { get; set; } = false;
    }

    public class ProjectFileContent : ProjectFile
    {
        public ProjectFileContent() { }
        public ProjectFileContent(ProjectFile file, string? content)
        {
            Id = file.Id;
            Name = file.Name;
            RelativePath = file.RelativePath;
            Content = content;
            ReadOnly = file.ReadOnly;
        }
        public string? Content { get; set; }
    }

    public class ProjectFileLines : ProjectFile
    {
        public ProjectFileLines()
        {
            Lines = new Dictionary<int, string>();
        }

        public ProjectFileLines(ProjectFile file, string? content)
        {
            Id = file.Id;
            Name = file.Name;
            RelativePath = file.RelativePath;
            ReadOnly = file.ReadOnly;
            Lines = new Dictionary<int, string>();
            if (!string.IsNullOrEmpty(content))
            {
                var lines = content.Split(Environment.NewLine);
                for(int i = 0; i < lines.Length; i++)
                {
                    Lines[i] = lines[i].Trim();
                }
            }
        }

        public Dictionary<int, string> Lines { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var line in Lines.OrderBy(x => x.Key))
                sb.AppendLine(line.Value);
            return sb.ToString();
        }
    }
}
