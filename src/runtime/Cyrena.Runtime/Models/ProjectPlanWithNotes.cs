using Cyrena.Models;

namespace Cyrena.Runtime.Models
{
    public class ProjectPlanWithNotes : ProjectPlan
    {
        public ProjectPlanWithNotes(string rootDirectory) : base(rootDirectory)
        {
            Files = new List<ProjectFile>();
            Folders = new List<ProjectFolder>();
            Notes = new List<NoteRecord>();
        }

        public ProjectPlanWithNotes(ProjectPlan plan) : base(plan.RootDirectory)
        {
            Files = plan.Files;
            Folders = plan.Folders;
            Notes = new List<NoteRecord>();
        }

        public IEnumerable<NoteRecord> Notes { get; set; }
    }
}
