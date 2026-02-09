{
  "Id": "components_shared_LoadingSpinner",
  "Title": "LoadingSpinner Component Specification",
  "Summary": "Reusable loading spinner component for Blazor Razor Class Library",
  "Keywords": [
    "LoadingSpinner",
    "UI",
    "Component",
    "Reusable",
    "Spinner"
  ],
  "Content": "The **LoadingSpinner** component is a reusable UI element that displays a visual indicator while asynchronous operations are in progress. It is intended for consumption by any project that references this library. The component exposes no parameters; it simply renders a CSS‑based spinner animation. The component is placed in `Components/Shared` and can be used in any Razor page or component via `<LoadingSpinner />`.\n\n**Usage**:\n```razor\n<LoadingSpinner />\n```\n\nThe component relies on the default CSS defined in `wwwroot/css/LoadingSpinner.css` (or inline styles if preferred). It does not depend on any services or contracts, keeping it lightweight and free of business logic.\n\n**Purpose**: Provide a consistent loading indicator across consuming applications without requiring custom implementation.\n\n**Integration**: No additional DI or configuration is required. Simply reference the component in markup.\n",
  "Link": null,
  "FilePath": null
}