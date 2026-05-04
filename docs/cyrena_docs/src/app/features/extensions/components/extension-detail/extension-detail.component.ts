import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ExtensionInfoComponent } from '../extension-info/extension-info.component';

const APP_ID_MAP: Record<string, string> = {
  'cyrena.api_references': '11486102-aa19-43d7-be0c-70aba7d9a51a',
  'cyrena.developer': '11486102-aa19-43d7-be0c-70aba7d9a51a',
  'cyrena.dotnet.csharp': '11486102-aa19-43d7-be0c-70aba7d9a51a',
  'cyrena.platformio': '11486102-aa19-43d7-be0c-70aba7d9a51a',
  'cyrena.angular': '11486102-aa19-43d7-be0c-70aba7d9a51a',
  'cyrena.arduino_ide': '11486102-aa19-43d7-be0c-70aba7d9a51a',
  'cyrena.tavily': 'ead9f857-5111-4759-a1f9-c75361e0a347'
};

@Component({
  standalone: true,
  imports: [CommonModule, RouterLink, ExtensionInfoComponent],
  templateUrl: './extension-detail.component.html',
  styleUrl: './extension-detail.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExtensionDetailComponent {
  private readonly route = inject(ActivatedRoute);

  readonly packageId = computed(() => this.route.snapshot.paramMap.get('packageId') || '');
  readonly applicationId = computed(() => APP_ID_MAP[this.packageId()] || '');
  readonly isTavily = computed(() => this.packageId() === 'cyrena.tavily');
  readonly isDotnet = computed(() => this.packageId() === 'cyrena.dotnet.csharp');
  readonly isPlatformio = computed(() => this.packageId() === 'cyrena.platformio');
  readonly isAngular = computed(() => this.packageId() === 'cyrena.angular');
  readonly isArduino = computed(() => this.packageId() === 'cyrena.arduino_ide');
}
