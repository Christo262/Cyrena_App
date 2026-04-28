import { Routes } from '@angular/router';
import { DocLayoutComponent } from './components/doc-layout/doc-layout.component';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/home/components/home-page/home-page.component').then(m => m.HomePageComponent)
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
        path: 'extensions',
        loadComponent: () => import('./features/docs/components/extension-development/extension-development.component').then(m => m.ExtensionDevelopmentComponent)
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
