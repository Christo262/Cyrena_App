namespace Cyrena.Coding.Models
{
    /// <summary>
    /// View model for the LLM of the <see cref="DevelopPlan"/> and the <see cref="StickyNote"/>
    /// </summary>
    public class DevelopPlanAIModel : DevelopPlan
    {
        public DevelopPlanAIModel(string rootDirectory) : base(rootDirectory)
        {
            Files = new List<DevelopFile>();
            Folders = new List<DevelopFolder>();
            Notes = new List<StickyNote>();
        }

        public DevelopPlanAIModel(DevelopPlan plan) : base(plan.RootDirectory)
        {
            Files = plan.Files;
            Folders = plan.Folders;
            Notes = new List<StickyNote>();
            AllowedFileTypes = plan.AllowedFileTypes;
        }

        public List<StickyNote> Notes { get; set; }
    }
}
