import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

interface NavItem {
  label: string;
  route: string;
  external?: boolean;
}

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NavbarComponent {
  readonly navItems = signal<NavItem[]>([
    { label: 'Home', route: '/index.html', external: true },
    { label: 'Documentation', route: '/docs' },
    { label: 'Developer', route: '/developer' },
    { label: 'Getting Started', route: '/docs/getting-started' }
  ]);

  readonly isMenuOpen = signal(false);

  toggleMenu(): void {
    this.isMenuOpen.update(v => !v);
  }
}
