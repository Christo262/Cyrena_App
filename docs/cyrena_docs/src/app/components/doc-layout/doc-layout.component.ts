import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { DocPage } from '../../models/doc-pagemodel.model';

@Component({
  selector: 'app-doc-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './doc-layout.component.html',
  styleUrl: './doc-layout.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DocLayoutComponent {
  readonly sidebarOpen = signal(true);

  readonly docPages = signal<DocPage[]>([
    {
      id: 'getting-started',
      title: 'Getting Started',
      description: 'Installation and first steps',
      route: '/docs/getting-started',
      icon: 'bi-rocket'
    },
    {
      id: 'ui-overview',
      title: 'UI Overview',
      description: 'A quick tour of the Cyréna interface',
      route: '/docs/ui-overview',
      icon: 'bi-window-desktop'
    },
    {
      id: 'dotnet',
      title: '.NET Development',
      description: 'Build .NET projects with Cyrena',
      route: '/docs/dotnet',
      icon: 'bi-filetype-cs'
    },
    {
      id: 'arduino',
      title: 'Arduino IDE',
      description: 'Arduino IDE sketch support',
      route: '/docs/arduino',
      icon: 'bi-motherboard'
    },
    {
      id: 'platformio',
      title: 'PlatformIO',
      description: 'PlatformIO project support',
      route: '/docs/platformio',
      icon: 'bi-cpu'
    },
    {
      id: 'angular',
      title: 'Angular',
      description: 'Angular project scaffolding and code generation',
      route: '/docs/angular',
      icon: 'bi-filetype-js'
    },
    {
      id: 'api-references',
      title: 'API References',
      description: 'Shippable AI memory for libraries',
      route: '/docs/api-references',
    }
  ]);

  toggleSidebar(): void {
    this.sidebarOpen.update(v => !v);
  }
}
