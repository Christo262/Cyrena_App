# Model Reviews

A quick look at how different LLMs perform inside my custom desktop development‑agent app.

---

## Quick‑Reference Table

| Category | Model (provider)                | Size | Best‑use case(s)                 | Thinking mode recommendation | Key Strengths / Weaknesses |
|----------|--------------------------------|------|----------------------------------|------------------------------|----------------------------|
| Offline  | **Ollama – gpt‑oss:20B**        | 20 B | General chat, Arduino/PlatformIO (arduino framework) | Low (medium → slow, high → over‑thinking) | ✅ Good for simple Arduino sketches. <br>❌ Obsolete ESP‑IDF code; needs explicit web‑search. |
| Offline  | **Ollama – qwen3‑coder:30B**   | 30 B | Embedded dev (Arduino, ESP‑IDF), .NET (simple) | Low‑Medium (high → verbose, long) | ✅ Better ESP‑IDF awareness than 20B. <br>❌ Talks aloud; rarely searches on its own. |
| Offline  | **Ollama – qwen2.5‑coder:14B** / **llama3.2:3B** | 14 B / 3 B | Pure chat | N/A | Too small for meaningful code generation or function calling. |
| Cloud    | **Ollama – gpt‑oss:120B‑cloud**| 120 B| Chat and most dev tasks | Low‑Medium (high → very slow) | ✅ Produces code quickly. <br>❌ Occasionally omits parts for brevity (needs a reminder). |
| Cloud    | **OpenAI – gpt‑5**             | –    | All scenarios | – | ⭐️ Outstanding overall; only occasional prompts needed for web‑search or newer .NET versions. |
| Cloud    | **Ollama – qwen3‑coder:480B‑cloud**| 480 B| .NET (tried) | – | ⚠️ Very unstable – “feels threatened”, skips code, invents nonsense, rarely calls functions. |
| Cloud    | **Ollama – deepseek‑v3.1:671B‑cloud**| 671 B| Cyréna (experimental) | – | 🐢 Extremely lazy – will not act unless forced to do a web query, then gives generic “I did the search”. |
| Cloud    | **Ollama – minimax‑m2.7:cloud** | –    | Single‑project .NET | – | 👍 My favourite – works well when given very explicit, structured instructions (sticky notes, API‑reference overview). Great for function calling. |

---

## Detailed Reviews

### 1. Offline Models

#### **Ollama – gpt‑oss:20B**
* **Chat** – Excellent for casual conversation.  
* **Thinking parameter** – Keep it on **Low**. Medium becomes sluggish; High makes the model “over‑think”.  
* **Arduino/PlatformIO** – Works nicely when the project uses only the `arduino` framework.  
* **ESP‑IDF** – Tends to output outdated or obsolete code. It will only search the web if you explicitly ask it to.  
* **.NET** – Handles simple errors well, provided there are clear API references. Struggles with larger, more complex solutions.

#### **Ollama – qwen3‑coder:30B**
* **Chat** – Not ideal; the model loves to “think out loud”.  
* **Embedded dev** – Better than the 20B model for ESP‑IDF, but still needs a manual prompt to look up recent changes.  
* **.NET** – Similar to gpt‑oss:20B but can manage slightly more complexity. The “thinking out loud” habit can cause long, meandering responses.

#### **Ollama – qwen2.5‑coder:14B & llama3.2:3B**
* Both are **too small** for serious code generation or function calling.  
* Use them strictly for **“Just Chatting”** or quick brainstorming.

---

### 2. Cloud Models  

> **Note:** Every cloud model I tested that is ≥ 120 B responds well to casual conversation.

#### **Ollama – gpt‑oss:120B‑cloud**
* Works well with **Low** or **Medium** thinking.  
* **Heavy** mode is far too slow.  
* Occasionally **omits code** for brevity, as if it “forgot” it was writing code—remind it to be exhaustive.

#### **OpenAI – gpt‑5**
* **Outstanding** across all use‑cases (chat, Arduino, ESP‑IDF, .NET, etc.).  
* Occasionally needs a prompt to perform a web search, especially for brand‑new releases (e.g., `.NET 10.0`).  
* Rarely refuses to answer; when it does, a short “look it up online” cue resolves the issue.

#### **Ollama – qwen3‑coder:480B‑cloud**
* **Very poor experience** (only tried with .NET).  
* The model becomes defensive, starts cutting code, and fabricates details.  
* Function calling is unreliable – it may describe an intention without actually invoking it.

#### **Ollama – deepseek‑v3.1:671B‑cloud**
* **Lazy** – responds to the prompt but then does nothing until you explicitly say “do the research”.  
* Even after a web query it replies with a vague “I did the search” and provides no concrete results.

#### **Ollama – minimax‑m2.7:cloud**
* **My current favourite** for single‑project .NET development.  
* Success hinges on **very explicit, structured prompts**:  
  * Use **sticky notes** to capture hard rules.  
  * Keep an **“Overview”** of the application in an “API References” sticky note.  
* With these constraints the model can:
  * Build entire applications that are easy to test, maintain, and debug.  
  * Continuously update the API References & sticky notes as it learns, making it ideal for **function calling**.

---

## General Tips for Getting the Most Out of These Models

1. **Set the thinking level if the model supports it** – low for speed, medium for more thoroughness, high only when you truly need deep reasoning (and are okay with longer latency).  
2. **Force web searches when you suspect outdated knowledge** – most models will not search unless you ask. Example prompt: Please look up the latest ESP‑IDF API changes and summarize them.
3. **Structure your prompts** for the larger cloud models (especially minimax‑m2.7):  
	* Begin with a **brief overview** of the project.  
	* List **hard rules** as bullet points and instruct the model to make these “sticky notes”.  
	* Provide any relevant **API reference**.  
	* End with a clear **action request**.
4. **When a model omits code** (only happens with a certain few models), gently remind it: Please include the full implementation of the function; do not skip any lines.

