import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

interface Guide {
  title: string;
  description: string;
  difficulty: 'Beginner' | 'Intermediate' | 'Advanced';
  estimatedTime: string;
}

@Component({
  selector: 'app-guides',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './guides.component.html',
  styleUrl: './guides.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GuidesComponent {
  readonly guides = signal<Guide[]>([
    {
      title: 'Building Your First Chatbot',
      description: 'Learn how to create a simple conversational AI chatbot using the Cyréna SDK.',
      difficulty: 'Beginner',
      estimatedTime: '15 min'
    },
    {
      title: 'Creating Custom Agents',
      description: 'Design specialized agents with custom instructions and behavior for specific use cases.',
      difficulty: 'Beginner',
      estimatedTime: '20 min'
    },
    {
      title: 'Integrating Knowledge Bases',
      description: 'Connect your documents and data sources to ground your AI responses in real information.',
      difficulty: 'Intermediate',
      estimatedTime: '25 min'
    },
    {
      title: 'Building Tool-Enabled Agents',
      description: 'Extend your agents with custom tools that can interact with external APIs and services.',
      difficulty: 'Intermediate',
      estimatedTime: '30 min'
    },
    {
      title: 'Multi-Agent Workflows',
      description: 'Orchestrate multiple agents working together to solve complex tasks.',
      difficulty: 'Advanced',
      estimatedTime: '45 min'
    },
    {
      title: 'Production Deployment',
      description: 'Best practices for deploying Cyréna-powered applications to production environments.',
      difficulty: 'Advanced',
      estimatedTime: '40 min'
    }
  ]);

  readonly difficultyClass = (difficulty: string): string => {
    switch (difficulty) {
      case 'Beginner': return 'bg-success';
      case 'Intermediate': return 'bg-warning text-dark';
      case 'Advanced': return 'bg-danger';
      default: return 'bg-secondary';
    }
  };
}
