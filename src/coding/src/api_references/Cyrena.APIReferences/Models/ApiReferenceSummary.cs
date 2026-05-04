using Cyrena.Models;
using Newtonsoft.Json;

namespace Cyrena.APIReferences.Models
{
    public class ApiReferenceSummary : IJsonSerializable
    {
        public ApiReferenceSummary(string id, string? title, string? summary)
        {
            Id = id;
            Title = title;
            Summary = summary;
        }

        public string Id { get; set; }
        public string? Title { get; set; }
        public string? Summary { get; set; }
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
