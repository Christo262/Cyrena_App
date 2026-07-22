import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { AboutComponent } from './components/about/about.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'about', component: AboutComponent },
  { path: 'docs', loadComponent: () => import('./features/documentation/components/docs-landing-page/docs-landing-page.component').then(m => m.DocsLandingPageComponent) },
  { path: 'docs/ui-overview', loadComponent: () => import('./features/documentation/components/ui-overview-doc/ui-overview-doc.component').then(m => m.UiOverviewDocComponent) },
  { path: 'model-reviews', loadComponent: () => import('./features/model-reviews/components/model-reviews-page/model-reviews-page.component').then(m => m.ModelReviewsPageComponent) },
  { path: '**', redirectTo: '' }
];
