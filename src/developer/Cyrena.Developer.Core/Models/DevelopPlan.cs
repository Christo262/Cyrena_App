using Cyrena.Models;
using Newtonsoft.Json;

namespace Cyrena.Developer.Models
{
    public class DevelopPlan : IJsonSerializable
    {
        public DevelopPlan(string rootDirectory)
        {
            Files = new List<DevelopFile>();
            Folders = new List<DevelopFolder>();
            RootDirectory = rootDirectory;
            DataDirectory = Path.Combine(rootDirectory, ".cyrena");
        }

        [JsonConstructor]
        internal DevelopPlan()
        {
            Files = new List<DevelopFile>();
            Folders = new List<DevelopFolder>();
            RootDirectory = string.Empty;
            DataDirectory = string.Empty;
        }

        [JsonIgnore]
        public string RootDirectory { get; set; }
        [JsonIgnore]
        public string DataDirectory { get; set; }

        public List<DevelopFile> Files { get; set; }
        public List<DevelopFolder> Folders { get; set; }

        public static bool TryLoadFromDirectory(string dir, out DevelopPlan plan)
        {
            try
            {
                var path = Path.Combine(dir, ".cyrena", "plan");
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var pl = JsonConvert.DeserializeObject<DevelopPlan>(json);
                    if (pl == null)
                    {
                        plan = new DevelopPlan(dir);
                        return false;
                    }
                    pl.RootDirectory = dir;
                    plan = pl;
                    return true;
                }
                plan = new DevelopPlan(dir);
                return false;
            }
            catch
            {
                plan = new DevelopPlan(dir);
                return false;
            }
        }

        public static void Save(DevelopPlan plan)
        {
            var path = Path.Combine(plan.DataDirectory, "plan");
            File.WriteAllText(path, plan.ToString());
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public override string ToString()
        {
            return ToJson();
        }
    }
}
