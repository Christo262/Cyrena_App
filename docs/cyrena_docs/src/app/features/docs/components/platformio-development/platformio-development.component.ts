import { ChangeDetectionStrategy, Component } from '@angular/core';

interface Step {
  number: number;
  title: string;
  description: string;
}

interface CoreLayoutEntry {
  item: string;
  content: string;
  access: string;
}

interface EspIdfEntry {
  item: string;
  content: string;
  access: string;
}

interface TaskEntry {
  task: string;
  description: string;
}

@Component({
  selector: 'app-platformio-development',
  standalone: true,
  imports: [],
  templateUrl: './platformio-development.component.html',
  styleUrl: './platformio-development.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PlatformioDevelopmentComponent {
  readonly extensionId = 'cyrena.platformio';

  readonly prerequisites = [
    'Visual Studio Code installed',
    'PlatformIO extension added to VS Code',
    'Cyréna PlatformIO extension (cyrena.platformio) installed and enabled'
  ];

  readonly coreLayout: CoreLayoutEntry[] = [
    { item: 'src', content: 'All sub-folders; .c, .cpp, .h source files', access: 'Read / write' },
    { item: 'include', content: 'All sub-folders; header (.h) files', access: 'Read / write' },
    { item: 'lib', content: 'All sub-folders; .c, .cpp, .h library files', access: 'Read-only' },
    { item: 'platformio.ini', content: 'Project configuration file', access: 'Read-only' }
  ];

  readonly espIdfLayout: EspIdfEntry[] = [
    { item: 'managed_components', content: 'All sub-folders; .c, .cpp, .h files', access: 'Read-only' },
    { item: 'components', content: 'All sub-folders; .c, .cpp, .h files', access: 'Read-only' },
    { item: 'sdkconfig*', content: 'ESP-IDF configuration files', access: 'Read-only' }
  ];

  readonly steps: Step[] = [
    {
      number: 1,
      title: 'Create a PlatformIO project',
      description: 'Set up a new project in Visual Studio Code using the PlatformIO extension.'
    },
    {
      number: 2,
      title: 'Open Cyréna and start a New Chat',
      description: 'Launch the Cyréna desktop app and create a new chat session.'
    },
    {
      number: 3,
      title: 'Expand the Embedded shortcuts',
      description: 'Click the Embedded category to reveal available shortcuts.'
    },
    {
      number: 4,
      title: 'Click PlatformIO',
      description: 'Select the PlatformIO shortcut to open the configuration dialog.'
    },
    {
      number: 5,
      title: 'Configure the chat',
      description: 'Enter a title, provide the path to your platformio.ini file, choose an AI connection, and optionally enable or disable specific Cyréna features.'
    },
    {
      number: 6,
      title: 'Press Submit',
      description: 'Cyréna indexes your project and opens the chat session.'
    },
    {
      number: 7,
      title: 'Begin chatting with the AI',
      description: 'Ask for code reviews, add or modify source files, resolve build issues, or ask any other project-specific questions.'
    }
  ];

  readonly tasks: TaskEntry[] = [
    { task: 'Code Review', description: 'Request a review of your source files for quality and correctness.' },
    { task: 'Add or Modify Files', description: 'Ask the AI to create new source files or update existing ones.' },
    { task: 'Resolve Build Issues', description: 'Get help diagnosing and fixing compilation or linking errors.' },
    { task: 'Project Questions', description: 'Ask anything about your PlatformIO project configuration or structure.' }
  ];
}
