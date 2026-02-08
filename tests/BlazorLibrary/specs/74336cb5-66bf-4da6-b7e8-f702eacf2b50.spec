{
  "Id": "74336cb5-66bf-4da6-b7e8-f702eacf2b50",
  "Title": "CounterCard Component Specification",
  "Summary": "Reusable counter card component for dashboards.",
  "Keywords": [
    "CounterCard",
    "dashboard",
    "component",
    "UI"
  ],
  "Content": "The CounterCard component is a reusable UI element that displays a numeric count, an optional title, and an optional link button. It is intended for use in dashboard pages where a quick summary of a metric is required.\n\n**Parameters**\n- `Title` (string): The heading displayed above the count. Defaults to \"Counter\".\n- `Count` (int): The numeric value to display.\n- `LinkUrl` (string): When provided, a button is rendered that navigates to the specified URL.\n- `LinkText` (string): The text shown on the link button. Defaults to \"Go\".\n\n**Styling**\n- The component uses Bootstrap card classes and a custom `counter-card` class defined in `wwwroot/css/dashboard.css`.\n- The surrounding layout should include the `dashboard.css` stylesheet for consistent spacing and colors.\n\n**Usage**\n```razor\n<CounterCard Title=\"Total Users\" Count=\"1234\" LinkUrl=\"/users\" LinkText=\"View Users\" />\n```\n\n**Dependencies**\n- Requires Bootstrap CSS for card styling.\n- Requires `wwwroot/css/dashboard.css` to be referenced in the consuming project.\n\n**Notes**\n- The component is intentionally lightweight and contains no business logic; it purely renders data passed via parameters.\n- It is designed to be dropped into any Razor page or component without additional configuration.\n",
  "Link": null,
  "FilePath": null
}