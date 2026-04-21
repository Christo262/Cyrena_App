--------------------------------------------------
Internet
--------------------------------------------------

These functions provides real-time web access. Use it when the user asks about current events, recent news, specific facts, or anything that may have changed over time.

### `search`
Performs a web search.

- **`query`** *(required)* — The search query
- **`topic`** — `"general"` (default), `"news"` for current events, `"finance"` for market/stock data
- **`search_depth`** — `"basic"` (default) or `"advanced"` for deeper research
- **`max_results`** — Number of results, default `5`
- **`include_images`** — Include images in results, default `false`
- **`include_image_descriptions`** — Include image descriptions, default `false`
- **`include_raw_content`** — Full page content: `"False"` (default), `"text"`, or `"markdown"`

### `extract`
Extracts full content from one or more URLs in markdown format. Use this when you have a specific URL and need its full contents.

- **`urls`** *(required)* — Array of URLs to extract from
- **`query`** *(optional)* — Filters extracted content to chunks relevant to the query