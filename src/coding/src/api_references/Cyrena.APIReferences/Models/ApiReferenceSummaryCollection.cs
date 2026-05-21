using Cyrena.Models;

namespace Cyrena.APIReferences.Models
{
    public class ApiReferenceSummaryCollection : List<ApiReferenceSummary>, ISuppressibleResult
    {
        public ApiReferenceSummaryCollection() { }
        public ApiReferenceSummaryCollection(IEnumerable<ApiReferenceSummary> ext):base(ext) { }

        public string Suppress()
        {
            if (Count == 0)
                return "[APIREF_LIST: no references]";

            var arr = this
                .Select(x => $"[APIREF:{x.Id}; title:{x.Title ?? "untitled"}; summary omitted; use API_reference_read/API_reference_search]")
                .ToArray();
            return string.Join("\n", arr);
        }
    }
}
