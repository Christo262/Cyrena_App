using System.Text;

namespace Cyrena.Models
{
    public class ProjectFile : ProjectItem
    {
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
