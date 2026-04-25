import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet } from "../../node_modules/@angular/router/index";

@Component({
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet]
})
export class AppComponent {
  title = 'AngularTest';
}
