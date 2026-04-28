import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

interface Feature {
  icon: string;
  title: string;
  description: string;
}

interface Domain {
  icon: string;
  name: string;
  items: string[];
}

interface NotItem {
  icon: string;
  label: string;
}

interface PlatformTrait {
  icon: string;
  title: string;
  description: string;
}

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomePageComponent {
  readonly platformTraits = signal<PlatformTrait[]>([
    {
      icon: 'bi-laptop',
      title: 'Desktop App',
      description: 'Runs natively on your machine. No browser tabs, no web dependencies, no server round-trips.'
    },
    {
      icon: 'bi-shield-lock',
      title: 'Offline-First',
      description: 'Your code never leaves your machine. Work entirely offline with local models via Ollama.'
    },
    {
      icon: 'bi-plug',
      title: 'Model-Agnostic',
      description: 'Not locked to one AI provider. Connect Ollama, OpenAI, or any compatible endpoint. Switch anytime.'
    },
    {
      icon: 'bi-hdd-network',
      title: 'No Server',
      description: 'Does not expose endpoints or talk to a central server. You own the infrastructure. You own the data.'
    }
  ]);

  readonly ollamaFeatures = signal<string[]>([
    'Run models entirely on your own hardware',
    'No internet connection required',
    'Your code stays on your machine',
    'Supports Llama, Mistral, CodeLlama, and more',
    'Free and open source'
  ]);

  readonly openaiFeatures = signal<string[]>([
    'State-of-the-art cloud models',
    'Always up to date with latest releases',
    'GPT-4, GPT-4o, and future models',
    'Fast inference with global infrastructure',
    'Pay-as-you-go pricing'
  ]);

  readonly features = signal<Feature[]>([
    {
      icon: 'bi-puzzle',
      title: 'Extensions',
      description: 'Expand Cyréna beyond coding with custom integrations, workflows, and business-specific tooling. Build what your team needs.'
    },
    {
      icon: 'bi-code-slash',
      title: 'Coding',
      description: 'Where it all started. Deep support for .NET, PlatformIO, Arduino — and now Angular too. More ecosystems on the way.'
    },
    {
      icon: 'bi-shield-lock',
      title: 'Offline-First & Model Agnostic',
      description: 'Work offline with local models via Ollama, or go online with OpenAI. Switch models in any chat at any time. Your choice, always.'
    },
    {
      icon: 'bi-lightbulb',
      title: 'Your Ideas',
      description: 'Build anything. From prototypes to production systems, Cyréna helps you turn ideas into working software.'
    }
  ]);

  readonly domains = signal<Domain[]>([
    {
      icon: 'bi-microsoft',
      name: '.NET Ecosystem',
      items: ['MVC', 'Blazor', 'Class Libraries', 'Razor Class Libraries']
    },
    {
      icon: 'bi-motherboard',
      name: 'Arduino',
      items: ['Arduino IDE']
    },
    {
      icon: 'bi-cpu',
      name: 'PlatformIO',
      items: ['ESP-IDF Framework', 'Arduino Framework']
    },
    {
      icon: 'bi-filetype-html',
      name: 'Angular',
      items: ['Standalone Components', 'Signals', 'Services', 'Routing']
    }
  ]);

  readonly notItems = signal<NotItem[]>([
    { icon: 'bi-x-lg', label: 'A chatbot' },
    { icon: 'bi-x-lg', label: 'A code snippet generator' },
    { icon: 'bi-x-lg', label: 'A cloud-dependent SaaS' },
    { icon: 'bi-x-lg', label: 'A replacement for understanding code' },
    { icon: 'bi-x-lg', label: 'A magic wand' },
    { icon: 'bi-x-lg', label: 'Afraid of your compiler errors' }
  ]);

  readonly platformFeatures = signal<string[]>([
    'Email integration',
    'Calendar management',
    'Custom workflows',
    'Internal tools',
    'Your own extensions',
    'Unified AI experience'
  ]);
}
