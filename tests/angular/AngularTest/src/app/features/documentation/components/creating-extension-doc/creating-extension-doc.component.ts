import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-creating-extension-doc',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './creating-extension-doc.component.html',
  styleUrl: './creating-extension-doc.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CreatingExtensionDocComponent {

}
