import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DocPage } from '../../../../models/doc-pagemodel.model';

@Component({
  selector: 'app-docs-landing',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './docs-landing.component.html',
  styleUrl: './docs-landing.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DocsLandingComponent {
  readonly docCards = signal<DocPage[]>([
    {
      id: 'ui-overview',
      title: 'UI Overview',
      description: 'A quick tour of the Cyréna interface — launch window, development tools, and adaptive layouts.',
      route: '/docs/ui-overview',
      icon: 'bi-window-desktop'
    },
    {
      id: 'dotnet',
      title: '.NET Development',
      description: 'Build .NET projects with Cyréna — project scaffolding, code generation, and debugging workflows.',
      route: '/docs/dotnet',
      icon: 'bi-filetype-cs'
    },
    {
      id: 'arduino',
      title: 'Arduino IDE',
      description: 'Write, compile, and upload Arduino sketches with full IDE integration.',
      route: '/docs/arduino',
      icon: 'bi-motherboard'
    },
    {
      id: 'platformio',
      title: 'PlatformIO',
      description: 'Cross-platform embedded development with PlatformIO project support.',
      route: '/docs/platformio',
      icon: 'bi-cpu'
    },
    {
      id: 'extensions',
      title: 'Extension Development',
      description: 'Build custom extensions to expand Cyréna with your own integrations and workflows.',
      route: '/docs/extensions',
      icon: 'bi-puzzle'
    }
  ]);
}
