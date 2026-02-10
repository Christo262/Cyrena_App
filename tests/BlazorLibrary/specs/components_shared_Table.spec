{
  "Id": "components_shared_Table",
  "Title": "Table Component Specification",
  "Summary": "Adds filtering and sorting to the shared Table component.",
  "Keywords": [
    "Table",
    "filter",
    "sort",
    "generic",
    "component"
  ],
  "Content": "The Table component in Components/Shared/Table.razor now supports filtering and sorting.\n\nKey features:\n- `FilterText` (string): When set, the component filters its items by checking if any property value contains the filter text (case‑insensitive).\n- `SortKeySelector` (Func<TItem, object>): When provided, the component sorts the items by the returned key. The sort is stable and uses Comparer<object>.Default.\n- `FilteredItems` and `SortedItems` are computed lazily and cached per render.\n- The component still renders a simple `<table>` with a header row and a body row for each item.\n\nUsage example:\n```razor\n<Table Items=\"myItems\" FilterText=\"search\" SortKeySelector=\"item => item.Name\" />\n```\n\nThe component is generic over `TItem` and can be used with any data type. It is fully documented and unit‑tested in the library’s test project.\n",
  "Link": null,
  "FilePath": null
}