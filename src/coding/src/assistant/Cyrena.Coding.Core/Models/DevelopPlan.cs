using Cyrena.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cyrena.Coding.Models
{
    public class DevelopPlan : ISuppressibleResult
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

        public string Suppress()
        {
            return $"[PLAN:omitted; use Project_get_plan]";
        }
    }
}
