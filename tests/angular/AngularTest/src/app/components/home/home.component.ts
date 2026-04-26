import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  template: '<h1>Home</h1>',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomeComponent {}
