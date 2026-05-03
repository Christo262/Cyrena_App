import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DocPage } from '../../../../models/doc-pagemodel.model';

@Component({
  selector: 'app-developer-landing',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './developer-landing.component.html',
  styleUrl: './developer-landing.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DeveloperLandingComponent {
  readonly devCards = signal<DocPage[]>([
    {
      id: 'cyrena-core',
      title: 'Cyrena.Core',
      description: 'Core contracts, models, builders, and extension methods for the Cyréna AI assistant framework.',
      route: '/developer/core',
      icon: 'bi-box-seam'
    },
    {
      id: 'cyrena-persistence-core',
      title: 'Cyrena.Persistence.Core',
      description: 'Persistence abstraction layer with generic repository IStore<T>, ICyrenaPersistenceBuilder, and LINQ query extensions.',
      route: '/developer/persistence',
      icon: 'bi-database'
    },
    {
      id: 'cyrena-extensa-core',
      title: 'Cyrena.Extensa.Core',
      description: 'Extension system contracts and models for dynamic plugin loading with dependency resolution.',
      route: '/developer/extensa',
      icon: 'bi-plug'
    },
    {
      id: 'cyrena-components-core',
      title: 'Cyrena.Components.Core',
      description: 'Blazor UI contracts, base classes, shared components, and extension methods for building UI elements.',
      route: '/developer/components',
      icon: 'bi-window'
    },
    {
      id: 'cyrena-coding-core',
      title: 'Cyrena.Coding.Core',
      description: 'Contracts, models, extensions, and configuration constants for project-aware AI coding capabilities.',
      route: '/developer/coding',
      icon: 'bi-code-slash'
    },
    {
      id: 'extension-development',
      title: 'Extension Development',
      description: 'Build custom extensions to expand Cyréna with your own integrations, workflows, and AI capabilities.',
      route: '/developer/extensions',
      icon: 'bi-puzzle'
    }
  ]);
}
