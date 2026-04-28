import { ChangeDetectionStrategy, Component, signal } from '@angular/core';

interface Concept {
  title: string;
  description: string;
  icon: string;
}

@Component({
  selector: 'app-core-concepts',
  standalone: true,
  imports: [],
  templateUrl: './core-concepts.component.html',
  styleUrl: './core-concepts.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CoreConceptsComponent {
  readonly concepts = signal<Concept[]>([
    {
      icon: 'bi-chat-dots',
      title: 'Conversations',
      description: 'Cyréna is built around the concept of conversations. A conversation is a sequence of messages exchanged between a user and the AI. Each message has a role (system, user, or assistant) and content.'
    },
    {
      icon: 'bi-layers',
      title: 'Agents',
      description: 'Agents are specialized AI configurations tailored for specific tasks. You can create custom agents with unique instructions, knowledge bases, and tool access to handle domain-specific workflows.'
    },
    {
      icon: 'bi-tools',
      title: 'Tools',
      description: 'Tools extend the capabilities of your agents by allowing them to interact with external systems. Define custom tools that your agents can invoke to perform actions like API calls, database queries, or file operations.'
    },
    {
      icon: 'bi-database',
      title: 'Knowledge Bases',
      description: 'Knowledge bases allow you to ground your agents in custom data. Upload documents, connect to data sources, and let your agents retrieve relevant information to provide accurate, context-aware responses.'
    },
    {
      icon: 'bi-shield-lock',
      title: 'Authentication',
      description: 'All API requests are authenticated using API keys. Keep your keys secure and rotate them regularly. Cyréna supports scoped keys with fine-grained permissions for different environments.'
    },
    {
      icon: 'bi-speedometer2',
      title: 'Rate Limiting',
      description: 'Cyréna implements rate limiting to ensure fair usage and platform stability. Limits vary by plan tier. Monitor your usage in the dashboard and set up alerts to stay within your quotas.'
    }
  ]);
}
