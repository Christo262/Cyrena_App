using Cyrena.Models;
using System.Text.Json;

namespace Cyrena.Coding.Models
{
    public abstract class DevelopItem : Entity
    {
        public string Name { get; set; } = null!;
        public string RelativePath { get; set; } = null!;
    }
}
