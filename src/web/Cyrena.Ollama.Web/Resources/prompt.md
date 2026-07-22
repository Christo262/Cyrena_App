--------------------------------------------------
Ollama Web
--------------------------------------------------

These functions provide real-time web access via Ollama Web. Use them when the user asks about current events, recent news, specific facts, or anything that requires up-to-date information.

### `OllWeb_search`
Performs a web search.

- **`query`** *(required)* — The search query to look up.
- **`max_results`** — Number of results to return, default `5`.

### `OllWeb_fetch`
Fetches the full content of a specific URL. Use this when you have a specific URL and need its full contents.

- **`url`** *(required)* — The full URL of the page to fetch.