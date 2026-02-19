namespace Cyrena.Developer.Models
{
    public class DevelopFolder : DevelopItem
    {
        public DevelopFolder()
        {
            Files = new List<DevelopFile>();
            Folders = new List<DevelopFolder>();
        }

        public List<DevelopFile> Files { get; set; }
        public List<DevelopFolder> Folders { get; set; }
    }
}
