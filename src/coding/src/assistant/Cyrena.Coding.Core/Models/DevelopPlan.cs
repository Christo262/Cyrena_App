using Cyrena.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cyrena.Coding.Models
{
    public class DevelopPlan : FileTypeAllowance, ISuppressibleResult
    {
        public DevelopPlan(string rootDirectory)
        {
            Files = [];
            Folders = [];
            RootDirectory = rootDirectory;
            DataDirectory = Path.Combine(rootDirectory, ".cyrena");
            Id = RootDirectory.Replace(@"\", "_").Replace("/", "_").Replace(".", "_");
        }

        [JsonConstructor]
        internal DevelopPlan()
        {
            Files = [];
            Folders = [];
            RootDirectory = string.Empty;
            DataDirectory = string.Empty;
        }

        [JsonIgnore] public string Id { get; } = null!;
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
