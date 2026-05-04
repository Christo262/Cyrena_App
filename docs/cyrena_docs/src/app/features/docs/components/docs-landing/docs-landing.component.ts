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
      title: '.NET Development (C#)',
      description: 'Build .NET projects with Cyréna — project scaffolding, code generation, and debugging workflows.',
      route: '/docs/dotnet',
      icon: 'bi-filetype-cs'
    },
    {
      id: 'angular',
      title: 'Angular',
      description: 'Scaffold Angular projects with enforced folder structure, standalone components, and signal-based state.',
      route: '/docs/angular',
      icon: 'bi-filetype-js'
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
      id: 'api-references',
      title: 'API References',
      description: 'Shippable AI memory — export and import .aiapi files for libraries and frameworks.',
      route: '/docs/api-references',
      icon: 'bi-journal-code'
    }
  ]);
}
