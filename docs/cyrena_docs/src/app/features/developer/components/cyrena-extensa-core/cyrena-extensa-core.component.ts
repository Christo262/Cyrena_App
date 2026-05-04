import { ChangeDetectionStrategy, Component } from '@angular/core';

interface LoadingStep {
  number: number;
  title: string;
  description: string;
}

@Component({
  selector: 'app-cyrena-extensa-core',
  standalone: true,
  imports: [],
  templateUrl: './cyrena-extensa-core.component.html',
  styleUrl: './cyrena-extensa-core.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CyrenaExtensaCoreComponent {
  readonly loadingSteps: LoadingStep[] = [
    {
      number: 1,
      title: 'Discovery',
      description: 'The Extensa loader scans extension directories for ExtensionInfo manifests.'
    },
    {
      number: 2,
      title: 'Dependency Resolution',
      description: 'Dependencies are resolved in topological order. Missing or incompatible dependencies prevent loading.'
    },
    {
      number: 3,
      title: 'Assembly Loading',
      description: 'Extension assemblies are loaded into the application context.'
    },
    {
      number: 4,
      title: 'Build Phase',
      description: 'BuildExtension(CyrenaBuilder) is called on each IExtension implementation.'
    },
    {
      number: 5,
      title: 'Service Provider Build',
      description: 'The DI container is built after all extensions have configured services.'
    },
    {
      number: 6,
      title: 'Run Phase',
      description: 'IStartupTask.RunAsync is called for all registered startup tasks.'
    }
  ];
}
