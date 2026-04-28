import { ChangeDetectionStrategy, Component, signal } from '@angular/core';

interface ApiEndpoint {
  method: string;
  path: string;
  description: string;
  request?: string;
  response?: string;
}

@Component({
  selector: 'app-api-reference',
  standalone: true,
  imports: [],
  templateUrl: './api-reference.component.html',
  styleUrl: './api-reference.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ApiReferenceComponent {
  readonly baseUrl = signal('https://api.cyrena.ai/v1');

  readonly endpoints = signal<ApiEndpoint[]>([
    {
      method: 'POST',
      path: '/chat/completions',
      description: 'Send a chat completion request to the AI model.',
      request: `{
  "model": "cyrena-1",
  "messages": [
    { "role": "system", "content": "You are a helpful assistant." },
    { "role": "user", "content": "Hello!" }
  ],
  "temperature": 0.7,
  "max_tokens": 256
}`,
      response: `{
  "id": "chatcmpl-abc123",
  "object": "chat.completion",
  "created": 1677652288,
  "model": "cyrena-1",
  "choices": [
    {
      "index": 0,
      "message": {
        "role": "assistant",
        "content": "Hello! How can I assist you today?"
      },
      "finish_reason": "stop"
    }
  ],
  "usage": {
    "prompt_tokens": 20,
    "completion_tokens": 10,
    "total_tokens": 30
  }
}`
    },
    {
      method: 'GET',
      path: '/models',
      description: 'List all available models and their capabilities.',
      response: `{
  "object": "list",
  "data": [
    {
      "id": "cyrena-1",
      "object": "model",
      "created": 1677610602,
      "owned_by": "cyrena"
    }
  ]
}`
    },
    {
      method: 'POST',
      path: '/embeddings',
      description: 'Generate embeddings for a given input text.',
      request: `{
  "model": "cyrena-embedding-1",
  "input": "The quick brown fox jumps over the lazy dog."
}`,
      response: `{
  "object": "list",
  "data": [
    {
      "object": "embedding",
      "embedding": [0.0023, -0.0091, 0.0156, ...],
      "index": 0
    }
  ],
  "model": "cyrena-embedding-1",
  "usage": {
    "prompt_tokens": 9,
    "total_tokens": 9
  }
}`
    }
  ]);

  readonly methodClass = (method: string): string => {
    switch (method) {
      case 'GET': return 'bg-success';
      case 'POST': return 'bg-primary';
      case 'PUT': return 'bg-warning text-dark';
      case 'DELETE': return 'bg-danger';
      case 'PATCH': return 'bg-info text-dark';
      default: return 'bg-secondary';
    }
  };
}
