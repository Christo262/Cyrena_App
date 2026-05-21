using Cyrena.Models;

namespace Cyrena.APIReferences.Models
{
    public class ApiReferenceSearchCollection : List<ApiReferenceSearch>, ISuppressibleResult
    {
        public ApiReferenceSearchCollection() { }
        public ApiReferenceSearchCollection(IEnumerable<ApiReferenceSearch> ext) : base(ext) { }

        public string Suppress()
        {
            if (Count == 0)
                return "[APIREF_SEARCH: no results]";

            var arr = this
                .OrderByDescending(x => x.Score)
                .Select(x => $"[APIREF:{x.Id}; score:{x.Score}; title:{x.Title ?? "untitled"}]")
                .ToArray();
            return string.Join("\n", arr);
        }
    }
}
