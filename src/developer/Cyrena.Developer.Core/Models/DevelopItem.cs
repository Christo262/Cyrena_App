using Cyrena.Models;
using Newtonsoft.Json;

namespace Cyrena.Developer.Models
{
    public abstract class DevelopItem : Entity, IJsonSerializable
    {
        public string Name { get; set; } = default!;
        public string RelativePath { get; set; } = default!;

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
