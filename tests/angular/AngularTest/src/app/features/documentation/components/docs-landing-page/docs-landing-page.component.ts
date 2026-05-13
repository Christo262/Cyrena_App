import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-docs-landing-page',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './docs-landing-page.component.html',
  styleUrl: './docs-landing-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DocsLandingPageComponent {
  readonly currentYear = new Date().getFullYear();
}
