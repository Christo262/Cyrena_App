{
  "Id": "a27bf63c-7cfb-4480-aa85-171068887e3a",
  "Title": "Heading Component Specification",
  "Summary": "Reusable Heading component with size, text, class, and attribute capture.",
  "Keywords": [
    "Heading",
    "Component",
    "Reusable",
    "UI",
    "Parameters"
  ],
  "Content": "The Heading component provides a reusable, stylable heading element that can be used across the library. It accepts a heading level (Size) from 1 to 6, a Text string for the heading content, optional CSS Class for additional styling, and captures arbitrary attributes via AdditionalAttributes.\n\nUsage:\n```\n<Heading Text=\"Page Title\" Size=\"1\" Class=\"my-heading\" />\n```\n\nThe component renders an `<h{Size}>` element with the provided attributes and classes. It inherits from ComponentBase and is located in the `Components/Shared` folder.\n",
  "Link": null,
  "FilePath": null
}