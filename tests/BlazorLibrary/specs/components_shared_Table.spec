{
  "Id": "components_shared_Table",
  "Title": "Table Component Specification",
  "Summary": "Reusable generic Table component for rendering collections.",
  "Keywords": [
    "Table",
    "Reusable",
    "Component",
    "Generic",
    "RenderFragment"
  ],
  "Content": "The Table component is a reusable UI element that renders a generic collection of items in an HTML table. It exposes two parameters:\n\n* `Items` – an `IEnumerable<T>` of the data to display.\n* `RowTemplate` – a `RenderFragment<T>` that defines how each row should be rendered.\n\nThe component is generic (`Table<T>`) so it can be used with any data type. It does not contain business logic; it simply iterates over `Items` and renders each row using `RowTemplate`. Consumers should provide the column headers and cell content via the `RowTemplate`.\n\nUsage example:\n```\n<Table Items=\"myItems\">\n    <RowTemplate Context=\"item\">\n        <tr>\n            <td>@item.Id</td>\n            <td>@item.Name</td>\n        </tr>\n    </RowTemplate>\n</Table>\n```\n\nThis component is intended for reuse across the library and any consuming projects. It is documented in the Project Specifications to guide AI agents on how to use it.\n",
  "Link": null,
  "FilePath": null
}