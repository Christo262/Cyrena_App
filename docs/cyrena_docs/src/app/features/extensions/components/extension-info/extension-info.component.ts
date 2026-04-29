import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ExtensionDataService } from '../../services/extension-data.service';
import { Package } from '../../models/extensionmodel.model';

@Component({
  selector: 'app-extension-info',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './extension-info.component.html',
  styleUrl: './extension-info.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExtensionInfoComponent {
  private readonly extensionData = inject(ExtensionDataService);

  readonly applicationId = input.required<string>();
  readonly packageId = input.required<string>();

  readonly package = signal<Package | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly iconUrl = computed(() => {
    return this.extensionData.getIconUrl(this.applicationId(), this.packageId());
  });

  readonly latestVersion = computed(() => {
    const pkg = this.package();
    if (!pkg || !pkg.versions.length) return null;
    return pkg.versions[0];
  });

  readonly osLabels: Record<string, string> = {
    win: 'Windows',
    mac: 'macOS',
    linux: 'Linux',
    android: 'Android'
  };

  readonly archLabels: Record<string, string> = {
    x64: 'x64',
    arm64: 'ARM64',
    x86: 'x86',
    armv7: 'ARMv7'
  };

  constructor() {
    effect(() => {
      const appId = this.applicationId();
      const pkgId = this.packageId();
      if (appId && pkgId) {
        this.loadPackage(appId, pkgId);
      }
    }, { allowSignalWrites: true });
  }

  private loadPackage(appId: string, pkgId: string): void {
    this.loading.set(true);
    this.error.set(null);

    this.extensionData.getPackage(appId, pkgId).subscribe({
      next: (pkg) => {
        this.package.set(pkg);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.error.set(err.status === 404 ? 'Extension not found.' : 'Failed to load extension details. Please try again later.');
        this.loading.set(false);
      }
    });
  }

  formatBytes(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  getDownloadUrl(version?: string): string {
    return this.extensionData.getDownloadUrl(this.applicationId(), this.packageId(), version);
  }
}
