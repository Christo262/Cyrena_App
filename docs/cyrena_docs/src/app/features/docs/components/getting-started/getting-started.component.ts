import { ChangeDetectionStrategy, Component } from '@angular/core';

interface Step {
  number: number;
  title: string;
  description: string;
  code?: string;
  codeLang?: string;
}

@Component({
  selector: 'app-getting-started',
  standalone: true,
  imports: [],
  templateUrl: './getting-started.component.html',
  styleUrl: './getting-started.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GettingStartedComponent {
  readonly downloadUrl = 'https://github.com/Christo262/Cyrena_App/releases';

  readonly platforms = [
    { name: 'Windows', arch: 'win-x64', icon: 'bi-windows' },
    { name: 'Linux', arch: 'linux-x64', icon: 'bi-ubuntu' },
    { name: 'Linux ARM', arch: 'linux-arm64', icon: 'bi-cpu' }
  ];

  readonly steps: Step[] = [
    {
      number: 1,
      title: 'Download Cyréna',
      description: 'Grab the latest release for your platform from GitHub. We are currently in alpha, so supported architectures are limited — more platforms will be added as we move toward stable.'
    },
    {
      number: 2,
      title: 'Launch the Application',
      description: 'Windows: double-click Cyrena.exe. Linux: make it executable and run it from the terminal.',
      code: 'chmod +x Cyrena.Desktop\n./Cyrena.Desktop',
      codeLang: 'bash'
    },
    {
      number: 3,
      title: 'Install WebKitGTK (Linux Only)',
      description: 'If Cyréna does not launch on Linux, you may need WebKitGTK — the rendering engine the desktop shell depends on.',
      code: 'sudo apt install libwebkit2gtk-4.1-0',
      codeLang: 'bash'
    },
    {
      number: 4,
      title: 'Open Settings',
      description: 'When the app opens, click the gear icon in the top-right corner (or the Settings button on the welcome screen) to open the settings panel.'
    },
    {
      number: 5,
      title: 'Configure a Connection',
      description: 'Choose your AI backend. You can add both and switch between them at any time — even mid-conversation.'
    },
    {
      number: 6,
      title: 'Set Your Default',
      description: 'Go to the Defaults tab and use the dropdown to select which connection Cyréna should use automatically when you start a new chat.'
    },
    {
      number: 7,
      title: 'Start Chatting',
      description: 'Click New Chat and start talking to your model. You can switch models or providers in any existing chat at any time.'
    }
  ];

  readonly ollamaParams = [
    'num_predict',
    'num_context',
    'temperature',
    'min_p',
    'top_k',
    'top_p',
    'repeat_penalty',
    'seed',
    'stop'
  ];
}
