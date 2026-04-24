namespace Cyrena.Developer.Models
{
    public class SolutionViewModel
    {
        public SolutionViewModel(string rootDirectory)
        {
            Projects = new List<ProjectModel>();
            RootDirectory = rootDirectory;
        }
        public string RootDirectory { get; }
        public List<ProjectModel> Projects { get; set; }
    }
}
