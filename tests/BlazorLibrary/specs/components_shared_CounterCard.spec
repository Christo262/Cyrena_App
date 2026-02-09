{
  "Id": "components_shared_CounterCard",
  "Title": "CounterCard Component Specification",
  "Summary": "Specification for the CounterCard reusable UI component.",
  "Keywords": [
    "CounterCard",
    "component",
    "UI",
    "dashboard"
  ],
  "Content": "The CounterCard component is a reusable UI element that displays a title, a numeric count, and an optional link. It is intended to be used in dashboards or summary views where a quick numeric indicator is needed.\n\n**Component Parameters**\n- `Title` (string): The heading displayed above the count. Defaults to \"Counter\".\n- `Count` (int): The numeric value to display. No default; must be supplied.\n- `LinkUrl` (string): Optional URL for a navigation button. If null or empty, the button is omitted.\n- `LinkText` (string): Text shown on the button. Defaults to \"Go\".\n\n**Usage Pattern**\n```razor\n<CounterCard Title=\"Active Users\" Count=\"123\" LinkUrl=\"/users\" LinkText=\"View\" />\n```\n\n**Styling**\nThe component uses Bootstrap card classes and a custom `counter-card` class defined in `wwwroot/css/dashboard.css`.\n\n**Dependencies**\nNo external services or contracts are required; all logic is contained within the component.\n\n**Extensibility**\nThe component can be extended by adding additional parameters or child content, but any new public surface should be documented with a new specification.\n",
  "Link": null,
  "FilePath": null
}