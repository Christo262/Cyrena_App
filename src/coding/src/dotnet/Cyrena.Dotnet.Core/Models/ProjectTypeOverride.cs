using Cyrena.Models;

namespace Cyrena.Dotnet.Models
{
    public sealed class ProjectTypeOverride : Entity
    {
        [System.Text.Json.Serialization.JsonConstructor]
        internal ProjectTypeOverride() { }

        public ProjectTypeOverride(string projectId)
        {
            Id = projectId;
        }

        public string? ProjectTypeId { get; set; }
    }
}
