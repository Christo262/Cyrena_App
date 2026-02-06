namespace Cyrena.Models
{
    public class ProjectFolder : ProjectItem
    {
        public ProjectFolder()
        {
            Files = new List<ProjectFile>();
            Folders = new List<ProjectFolder>();
        }

        public List<ProjectFile> Files { get; set; }
        public List<ProjectFolder> Folders { get; set; }
    }
}
