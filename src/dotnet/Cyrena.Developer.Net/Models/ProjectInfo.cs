namespace Cyrena.Developer.Models
{
    /// <summary>
    /// Represents detailed information about a project
    /// </summary>
    public class ProjectInfo
    {
        public string AbsolutePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectType { get; set; } = string.Empty;
        public bool Exists { get; set; }

        public override string ToString()
        {
            return $"{ProjectName} ({ProjectType}) - {(Exists ? "Found" : "Missing")}";
        }
    }
}
