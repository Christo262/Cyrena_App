using Newtonsoft.Json;

namespace Cyrena.Models
{
    public class ProjectPlan : JsonStringObject
    {
        public ProjectPlan(string rootDirectory)
        {
            Files = new List<ProjectFile>();
            Folders = new List<ProjectFolder>();
            RootDirectory = rootDirectory;
        }

        [JsonConstructor]
        internal ProjectPlan() 
        {
            Files = new List<ProjectFile>();
            Folders = new List<ProjectFolder>();
            RootDirectory = string.Empty;
        }

        [JsonIgnore]
        public string RootDirectory { get; set; }

        public List<ProjectFile> Files { get; set; }
        public List<ProjectFolder> Folders { get; set; }

        public static bool TryLoadFromDirectory(string dir, out ProjectPlan plan)
        {
            try
            {
                var path = Path.Combine(dir, Project.CyrenaDirectory, "plan");
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var pl = JsonConvert.DeserializeObject<ProjectPlan>(json);
                    if (pl == null)
                    {
                        plan = new ProjectPlan(dir);
                        return false;
                    }
                    pl.RootDirectory = dir;
                    plan = pl;
                    return true;
                }
                plan = new ProjectPlan(dir);
                return false;
            }
            catch
            {
                plan = new ProjectPlan(dir);
                return false;
            }
        }

        public static void Save(ProjectPlan plan)
        {
            var path = Path.Combine(plan.RootDirectory, Project.CyrenaDirectory, "plan");
            File.WriteAllText(path, plan.ToString());
        }
    }
}
