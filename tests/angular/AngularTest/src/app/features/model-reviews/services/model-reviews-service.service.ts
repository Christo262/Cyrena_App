import { Injectable } from '@angular/core';
import { ModelReview } from '../models/model-reviewmodel.model';

@Injectable({
  providedIn: 'root'
})
export class ModelReviewsServiceService {
  private readonly reviews: ModelReview[] = [
    {
      category: 'Offline',
      model: 'gpt-oss:20B',
      provider: 'Ollama',
      size: '20B',
      bestUseCases: ['General chat', 'Arduino/PlatformIO (arduino framework)'],
      thinkingMode: 'Low (medium → slow, high → over-thinking)',
      strengths: ['Good for simple Arduino sketches'],
      weaknesses: ['Obsolete ESP-IDF code; needs explicit web-search'],
      rating: 3,
      detailedReview:
        'Excellent for casual conversation. Keep thinking on Low. Works nicely when the project uses only the arduino framework. Tends to output outdated or obsolete ESP-IDF code. Handles simple .NET errors well, provided there are clear API references. Struggles with larger, more complex solutions.',
      proTip: 'Ask the model to create API References for updated lookups.'
    },
    {
      category: 'Offline',
      model: 'qwen3-coder:30B',
      provider: 'Ollama',
      size: '30B',
      bestUseCases: ['Embedded dev (Arduino, ESP-IDF)', '.NET (simple)'],
      thinkingMode: 'Low-Medium (high → verbose, long)',
      strengths: ['Better ESP-IDF awareness than 20B'],
      weaknesses: ['Talks aloud; rarely searches on its own'],
      rating: 3,
      detailedReview:
        'Not ideal for chat; the model loves to think out loud. Better than the 20B model for ESP-IDF, but still needs a manual prompt to look up recent changes. Similar to gpt-oss:20B for .NET but can manage slightly more complexity. The thinking out loud habit can cause long, meandering responses.'
    },
    {
      category: 'Offline',
      model: 'qwen2.5-coder:14B / llama3.2:3B',
      provider: 'Ollama',
      size: '14B / 3B',
      bestUseCases: ['Pure chat'],
      thinkingMode: 'N/A',
      strengths: ['Lightweight'],
      weaknesses: ['Too small for meaningful code generation or function calling'],
      rating: 1,
      detailedReview:
        'Both are too small for serious code generation or function calling. Use them strictly for “Just Chatting” or quick brainstorming.'
    },
    {
      category: 'Cloud',
      model: 'gpt-oss:120B-cloud',
      provider: 'Ollama',
      size: '120B',
      bestUseCases: ['Chat and most dev tasks'],
      thinkingMode: 'Low-Medium (high → very slow)',
      strengths: ['Produces code quickly'],
      weaknesses: ['Occasionally omits parts for brevity (needs a reminder)'],
      rating: 4,
      detailedReview:
        'Works well with Low or Medium thinking. Heavy mode is far too slow. Occasionally omits code for brevity, as if it forgot it was writing code—remind it to be exhaustive.'
    },
    {
      category: 'Cloud',
      model: 'gpt-5',
      provider: 'OpenAI',
      bestUseCases: ['All scenarios'],
      thinkingMode: '–',
      strengths: ['Outstanding overall; only occasional prompts needed for web-search or newer .NET versions'],
      weaknesses: ['Rarely refuses to answer; when it does, a short “look it up online” cue resolves the issue'],
      rating: 5,
      detailedReview:
        'Outstanding across all use-cases (chat, Arduino, ESP-IDF, .NET, etc.). Occasionally needs a prompt to perform a web search, especially for brand-new releases (e.g., .NET 10.0). Rarely refuses to answer; when it does, a short “look it up online” cue resolves the issue.'
    },
    {
      category: 'Cloud',
      model: 'qwen3-coder:480B-cloud',
      provider: 'Ollama',
      size: '480B',
      bestUseCases: ['.NET (tried)'],
      thinkingMode: '–',
      strengths: [],
      weaknesses: ['Very unstable – “feels threatened”, skips code, invents nonsense, rarely calls functions'],
      rating: 1,
      detailedReview:
        'Very poor experience (only tried with .NET). The model becomes defensive, starts cutting code, and fabricates details. Function calling is unreliable – it may describe an intention without actually invoking it.'
    },
    {
      category: 'Cloud',
      model: 'deepseek-v3.1:671B-cloud',
      provider: 'Ollama',
      size: '671B',
      bestUseCases: ['Cyréna (experimental)'],
      thinkingMode: '–',
      strengths: [],
      weaknesses: ['Extremely lazy – will not act unless forced to do a web query, then gives generic “I did the search”'],
      rating: 1,
      detailedReview:
        'Lazy – responds to the prompt but then does nothing until you explicitly say “do the research”. Even after a web query it replies with a vague “I did the search” and provides no concrete results.'
    },
    {
      category: 'Cloud',
      model: 'minimax-m2.7:cloud',
      provider: 'Ollama',
      bestUseCases: ['Single-project .NET'],
      thinkingMode: '–',
      strengths: ['Works well when given very explicit, structured instructions (sticky notes, API-reference overview)', 'Great for function calling'],
      weaknesses: ['Needs very explicit, structured prompts to perform well'],
      rating: 4,
      detailedReview:
        'Works well for single-project .NET development. Success hinges on very explicit, structured prompts: use sticky notes to capture hard rules, keep an “Overview” of the application in an “API References” sticky note. With these constraints the model can build entire applications that are easy to test, maintain, and debug, and continuously update the API References & sticky notes as it learns.'
    },
    {
      category: 'Cloud',
      model: 'kimi-k2.6',
      provider: 'Kimi',
      bestUseCases: ['All dev tasks'],
      thinkingMode: '–',
      strengths: ['Takes time to understand the project and structure before making changes', 'Never dives in blind'],
      weaknesses: [],
      rating: 5,
      detailedReview:
        'My current favourite across dev tasks. Stands out for taking time to understand the project and its structure before making any changes – it never just dives in blind. This careful, context-first approach means fewer mistakes and less back-and-forth correcting misunderstood scope.'
    }
  ];

  getReviews(): ModelReview[] {
    return this.reviews;
  }

  getOfflineReviews(): ModelReview[] {
    return this.reviews.filter(r => r.category === 'Offline');
  }

  getCloudReviews(): ModelReview[] {
    return this.reviews.filter(r => r.category === 'Cloud');
  }
}
