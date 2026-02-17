using Cyrena.Models;
using Newtonsoft.Json;

namespace Cyrena.Developer.Docs.Models
{
    public class ApiReference : Entity, IJsonSerializable
    {
        public string? Title { get; set; }
        public string[] Keywords { get; set; } = [];
        public string? Summary { get; set; }

        public string? Link { get; set; }
        public string? Content { get; set; }

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
