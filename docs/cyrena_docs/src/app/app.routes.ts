import { Routes } from '@angular/router';
import { DocLayoutComponent } from './components/doc-layout/doc-layout.component';
import { DeveloperLayoutComponent } from './features/developer/components/developer-layout/developer-layout.component';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/home/components/home-page/home-page.component').then(m => m.HomePageComponent)
  },
  {
    path: 'extensions',
    loadComponent: () => import('./features/extensions/components/extensions-landing/extensions-landing.component').then(m => m.ExtensionsLandingComponent)
  },
  {
    path: 'extensions/:packageId',
    loadComponent: () => import('./features/extensions/components/extension-detail/extension-detail.component').then(m => m.ExtensionDetailComponent)
  },
  {
    path: 'docs',
    component: DocLayoutComponent,
    children: [
      {
        path: '',
        loadComponent: () => import('./features/docs/components/docs-landing/docs-landing.component').then(m => m.DocsLandingComponent)
      },
      {
        path: 'ui-overview',
        loadComponent: () => import('./features/docs/components/ui-overview/ui-overview.component').then(m => m.UiOverviewComponent)
      },
      {
        path: 'getting-started',
        loadComponent: () => import('./features/docs/components/getting-started/getting-started.component').then(m => m.GettingStartedComponent)
      },
      {
        path: 'dotnet',
        loadComponent: () => import('./features/docs/components/dotnet-development/dotnet-development.component').then(m => m.DotnetDevelopmentComponent)
      },
      {
        path: 'arduino',
        loadComponent: () => import('./features/docs/components/arduino-development/arduino-development.component').then(m => m.ArduinoDevelopmentComponent)
      },
      {
        path: 'platformio',
        loadComponent: () => import('./features/docs/components/platformio-development/platformio-development.component').then(m => m.PlatformioDevelopmentComponent)
      },
      {
        path: 'angular',
        loadComponent: () => import('./features/docs/components/angular-development/angular-development.component').then(m => m.AngularDevelopmentComponent)
      },
      {
        path: 'api-references',
        loadComponent: () => import('./features/docs/components/api-references/api-references.component').then(m => m.ApiReferencesComponent)
      }
    ]
  },
  {
    path: 'developer',
    component: DeveloperLayoutComponent,
    children: [
      {
        path: '',
        loadComponent: () => import('./features/developer/components/developer-landing/developer-landing.component').then(m => m.DeveloperLandingComponent)
      },
      {
        path: 'extensions',
        loadComponent: () => import('./features/developer/components/extension-development/extension-development.component').then(m => m.ExtensionDevelopmentComponent)
      },
      {
        path: 'core',
        loadComponent: () => import('./features/developer/components/cyrena-core/cyrena-core.component').then(m => m.CyrenaCoreComponent)
      },
      {
        path: 'persistence',
        loadComponent: () => import('./features/developer/components/cyrena-persistence-core/cyrena-persistence-core.component').then(m => m.CyrenaPersistenceCoreComponent)
      },
      {
        path: 'extensa',
        loadComponent: () => import('./features/developer/components/cyrena-extensa-core/cyrena-extensa-core.component').then(m => m.CyrenaExtensaCoreComponent)
      },
      {
        path: 'components',
        loadComponent: () => import('./features/developer/components/cyrena-components-core/cyrena-components-core.component').then(m => m.CyrenaComponentsCoreComponent)
      },
      {
        path: 'coding',
        loadComponent: () => import('./features/developer/components/cyrena-coding-core/cyrena-coding-core.component').then(m => m.CyrenaCodingCoreComponent)
      }
    ]
  },
  {
    path: 'privacy',
    loadComponent: () => import('./components/privacy-page/privacy-page.component').then(m => m.PrivacyPageComponent)
  },
  {
    path: '**',
    redirectTo: ''
  }
];
