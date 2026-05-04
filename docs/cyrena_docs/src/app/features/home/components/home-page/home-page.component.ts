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

interface NewFeature {
  icon: string;
  title: string;
  tagline: string;
  description: string;
  image?: string;
  features: string[];
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
      icon: 'bi-puzzle',
      title: 'Extensible',
      description: 'Build extensions to adapt Cyréna to your workflow. Custom integrations, internal tools, and domain-specific agents — installed from a server or dropped in manually.'
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
    'Access to the latest OpenAI models',
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

  readonly newFeatures = signal<NewFeature[]>([
    {
      icon: 'bi-toggle-on',
      title: 'Feature Activation',
      tagline: 'The model only knows what it needs to know.',
      description: 'Feature Activation lets you enable or disable tools and capabilities per chat. When a feature is off, it does not just hide — it ceases to exist for the model entirely. No confusion, no accidental tool use, no noise. The agent operates with surgical precision on exactly what your current task requires.',
      image: 'images/guides/feature-activation.png',
      features: [
        'Enable or disable tools per chat',
        'Disabled features are invisible to the model',
        'No accidental tool calls or confusion',
        'Surgical precision for every task'
      ]
    },
    {
      icon: 'bi-file-earmark-text',
      title: 'Dynamic System Prompts',
      tagline: 'The right instructions, at the right time.',
      description: 'As features activate and deactivate, Cyréna\'s instruction set updates automatically. The agent always operates under the most relevant constraints for your current stack — Angular prompts for Angular work, firmware rules for firmware work. No static one-size-fits-all prompt. The context adapts with you.',
      features: [
        'Prompts update as features change',
        'Stack-specific constraints automatically applied',
        'No static, bloated system prompt',
        'Context that adapts to your current task'
      ]
    },
    {
      icon: 'bi-stack',
      title: 'Prompt Queuing',
      tagline: 'Load up your tasks. Go have a coffee.',
      description: 'Queue a sequence of instructions and let Cyréna work through them automatically. Each response completes before the next instruction fires. If something critical comes up mid-queue, Cyréna pauses and waits for your input before continuing. You stay in control without staying at your desk.',
      image: 'images/guides/prompt-queue.png',
      features: [
        'Queue multiple instructions in sequence',
        'Each response completes before the next fires',
        'Auto-pause on critical input required',
        'Work through tasks while you do something else'
      ]
    },
    {
      icon: 'bi-activity',
      title: 'Chat Status',
      tagline: 'Always know what\'s happening, at a glance.',
      description: 'Cyréna shows the live status of every chat directly in the sidebar. See which chats have context loaded and ready, which are actively working, and which are idle — without switching between them. No more wondering if the AI is still running or if a chat needs to be reopened.',
      image: 'images/guides/chat-status.png',
      features: [
        'Live status in the sidebar for every chat',
        'Unloaded — idle, context loads on open',
        'Loaded — context in memory and ready',
        'Working — AI is actively processing'
      ]
    }
  ]);
}
