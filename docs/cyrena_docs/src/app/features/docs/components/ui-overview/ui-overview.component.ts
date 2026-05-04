import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-ui-overview',
  standalone: true,
  imports: [],
  templateUrl: './ui-overview.component.html',
  styleUrl: './ui-overview.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UiOverviewComponent {

}
