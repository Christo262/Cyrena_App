import { Component, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

interface Feature {
  icon: string;
  title: string;
  description: string;
}

interface Domain {
  name: string;
  frameworks: string[];
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomeComponent {
  readonly features = signal<Feature[]>([
    {
      icon: '📋',
      title: 'Tasks',
      description: 'Create, track, and complete engineering tasks with an agent that understands your workflow.'
    },
    {
      icon: '💻',
      title: 'Coding',
      description: 'The core capability. Cyréna reads your code, understands constraints, and writes solutions that compile.'
    },
    {
      icon: '📄',
      title: 'Documents',
      description: 'Organize, search, and edit structured technical documents that become your codebase\'s memory.'
    },
    {
      icon: '💡',
      title: 'Your Ideas',
      description: 'Build anything. From internal tools to custom workflows — your organization gets a unified AI experience.'
    }
  ]);

  readonly domains = signal<Domain[]>([
    { name: '.NET', frameworks: ['MVC', 'Blazor', 'Class Libraries', 'Razor Class Libraries'] },
    { name: 'Arduino', frameworks: ['Arduino IDE'] },
    { name: 'PlatformIO', frameworks: ['ESP-IDF Framework', 'Arduino Framework'] }
  ]);

  readonly notList = signal<string[]>([
    'A chatbot',
    'A code snippet generator',
    'A cloud-dependent SaaS',
    'A replacement for understanding code',
    'A magic wand',
    'Afraid of your compiler errors'
  ]);
}
