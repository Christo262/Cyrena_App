import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

interface ProductBuild {
  architecture: string;
  size: string;
  downloadUrl: string | null;
  isAvailable: boolean;
}

interface DownloadProduct {
  id: string;
  name: string;
  tagline: string;
  description: string;
  icon: string;
  version: string;
  releaseDate: string;
  platforms: {
    windows?: ProductBuild[];
    linux?: ProductBuild[];
    macos?: ProductBuild[];
  };
  requirements: string[];
  screenshotUrl?: string | null;
}

@Component({
  selector: 'app-downloads-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './downloads-page.component.html',
  styleUrl: './downloads-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DownloadsPageComponent {
  readonly desktopProduct = signal<DownloadProduct>({
    id: 'desktop',
    name: 'Cyréna Desktop',
    tagline: 'Offline-first. Model-agnostic. Traditional windowed interface.',
    description:
      'Chat, extensions, coding tools, and all platform capabilities in a traditional windowed interface. Runs entirely on your machine with optional cloud model connections.',
    icon: 'bi-window-desktop',
    version: '1.0.0',
    releaseDate: 'Coming soon',
    platforms: {
      windows: [
        { architecture: 'x64', size: '~50 MB', downloadUrl: '/desktop/win-x64-setup.exe', isAvailable: true },
        { architecture: 'ARM64', size: '~50 MB', downloadUrl: '/desktop/win-arm64-setup.exe', isAvailable: true }
      ],
      linux: [
        { architecture: 'x64', size: '~50 MB', downloadUrl: '/desktop/linux-x64.zip', isAvailable: true },
        { architecture: 'ARM64', size: '~50 MB', downloadUrl: '/desktop/linux-arm64.zip', isAvailable: true }
      ],
      macos: [
        { architecture: 'Apple Silicon / Intel', size: '~50 MB', downloadUrl: null, isAvailable: false }
      ]
    },
    requirements: [
      '8 GB RAM minimum',
      '500 MB free disk space',
      'libwebkit2gtk-4.1-0 (Linux only)',
      'Ollama (optional, for local models)'
    ]
  });

  readonly hudProduct = signal<DownloadProduct>({
    id: 'hud',
    name: 'Cyréna HUD',
    tagline: 'A Heads Up Display that keeps Cyréna always within reach.',
    description:
      'A lightweight overlay that floats above your workspace for quick access to Cyréna without leaving your current window. All your chats, extensions, and AI capabilities in a compact UI. Configurable hotkey to show and hide — runs in the background, out of your way. Windows only.',
    icon: 'bi-layers',
    version: '1.0.0',
    releaseDate: 'Coming soon',
    platforms: {
      windows: [
        { architecture: 'x64', size: '~210 MB', downloadUrl: '/hud/win-x64/Cyrena.HUD.application', isAvailable: true }
      ]
    },
    requirements: [
      '8 GB RAM minimum'
    ],
    screenshotUrl: 'images/hud/screenshot.png'
  });
  readonly allReleasesUrl = signal<string>(
    'https://github.com/Christo262/Cyrena_App/releases'
  );

  readonly platformLabels: Record<string, { name: string; icon: string }> = {
    windows: { name: 'Windows', icon: 'bi-microsoft' },
    linux: { name: 'Linux', icon: 'bi-ubuntu' },
    macos: { name: 'macOS', icon: 'bi-apple' }
  };

  platformKeys(product: DownloadProduct): string[] {
    return Object.keys(product.platforms);
  }

  buildsForPlatform(product: DownloadProduct, platform: string): ProductBuild[] {
    return (product.platforms as Record<string, ProductBuild[]>)[platform] ?? [];
  }
}
