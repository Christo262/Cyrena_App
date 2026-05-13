import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-ui-overview-doc',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './ui-overview-doc.component.html',
  styleUrl: './ui-overview-doc.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UiOverviewDocComponent {

}
