using Newtonsoft.Json;

namespace Cyrena.Models
{
    /// <summary>
    /// Convenience to help serialize objects for LLM to understand result better
    /// </summary>
    public abstract class JsonStringObject
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
