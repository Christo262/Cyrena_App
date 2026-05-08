using Cyrena.Models;
using Newtonsoft.Json;

namespace Cyrena.Website.Models
{
    public class WebsiteViewModel : IJsonSerializable
    {
        public string WebsiteProjectId { get; set; } = default!;
        public string WebsiteName { get; set; } = default!;
        public string? Description { get; set; }

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
