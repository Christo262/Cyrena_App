import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { DocPage } from '../../../../models/doc-pagemodel.model';

@Component({
  selector: 'app-developer-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './developer-layout.component.html',
  styleUrl: './developer-layout.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DeveloperLayoutComponent {
  readonly sidebarOpen = signal(true);
  readonly devPages = signal<DocPage[]>([
    {
      id: 'cyrena-core',
      title: 'Cyrena.Core',
      description: 'Core contracts, models, builders, and extension methods.',
      route: '/developer/core',
      icon: 'bi-box-seam'
    },
    {
      id: 'cyrena-persistence-core',
      title: 'Cyrena.Persistence.Core',
      description: 'Persistence abstraction layer with IStore<T> and ICyrenaPersistenceBuilder.',
      route: '/developer/persistence',
      icon: 'bi-database'
    },
    {
      id: 'cyrena-extensa-core',
      title: 'Cyrena.Extensa.Core',
      description: 'Extension system contracts and models for dynamic plugin loading.',
      route: '/developer/extensa',
      icon: 'bi-plug'
    },
    {
      id: 'cyrena-components-core',
      title: 'Cyrena.Components.Core',
      description: 'Blazor UI contracts, base classes, and shared components.',
      route: '/developer/components',
      icon: 'bi-window'
    },
    {
      id: 'cyrena-coding-core',
      title: 'Cyrena.Coding.Core',
      description: 'Contracts, models, extensions, and configuration for project-aware AI coding.',
      route: '/developer/coding',
      icon: 'bi-code-slash'
    },
    {
      id: 'cyrena-voice-core',
      title: 'Cyrena.Voice.Core',
      description: 'Voice processing pipeline contracts and artifact models.',
      route: '/developer/voice',
      icon: 'bi-mic'
    },
    {
      id: 'extension-development',
      title: 'Extension Development',
      description: 'Build extensions for Cyréna',
      route: '/developer/extensions',
      icon: 'bi-puzzle'
    }
  ]);

  toggleSidebar(): void {
    this.sidebarOpen.update(v => !v);
  }
}