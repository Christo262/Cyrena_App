{
  "Id": "e3406fc1-aa5e-4bce-915c-001c7a38e8e0",
  "Title": "Project Intent Overview",
  "Summary": "High‑level overview of the reusable Blazor component library",
  "Keywords": [
    "project overview",
    "library purpose"
  ],
  "Content": "This library provides a set of reusable Blazor components and services that can be consumed by any Blazor application. The primary focus is on UI components that are independent of any domain logic, making them suitable for a wide range of scenarios such as dashboards, admin panels, or generic UI toolkits.\n\nKey components:\n- CounterCard: A simple card that displays a counter value and a button to increment it.\n- LoadingSpinner: A visual indicator for async operations.\n- Table: A generic table component that supports sorting and filtering.\n- Heading: A styled heading component.\n\nAll components reside in the Components folder hierarchy and expose minimal public parameters. They do not contain business logic; any state management should be handled by the consuming application or via injected services.\n\nThe library follows the standard Blazor Razor Class Library structure and can be referenced by any .NET project that supports Blazor.\n",
  "Link": null,
  "FilePath": null
}