import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-privacy-page',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './privacy-page.component.html',
  styleUrl: './privacy-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PrivacyPageComponent {
  readonly lastUpdated = 'January 2025';
}
