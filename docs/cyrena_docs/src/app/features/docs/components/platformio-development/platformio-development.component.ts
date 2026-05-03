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

interface FolderResponsibility {
  folder: string;
  location: string;
  purpose: string;
}

interface FeatureFolder {
  name: string;
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

  readonly srcLayout: FeatureFolder[] = [
    { name: 'main.c / main.cpp', description: 'Application entry point.' },
    { name: '{feature}/', description: 'Feature folder containing feature source, actions, and internals.' },
    { name: '  {feature}.c / {feature}.cpp', description: 'Feature initialisation or coordinator. Optional.' },
    { name: '  actions/', description: 'Public action implementations.' },
    { name: '  internals/', description: 'Private implementations. Never exposed outside the feature.' }
  ];

  readonly includeLayout: FeatureFolder[] = [
    { name: '{feature}/', description: 'Feature folder containing public headers, definitions, actions, and internals.' },
    { name: '  {feature}.h', description: 'The single public entry point for the feature. Consumers include only this file.' },
    { name: '  definitions/', description: 'Types, structs, enums, and constants. Never in src/.' },
    { name: '  actions/', description: 'Function declarations for public actions.' },
    { name: '  internals/', description: 'Private headers. Never included from outside their own feature.' }
  ];

  readonly folderResponsibilities: FolderResponsibility[] = [
    { folder: 'definitions/', location: 'include/{feature}/ only', purpose: 'Types, structs, enums, and constants. Never in src/.' },
    { folder: 'actions/', location: 'Both', purpose: 'Function declarations in include/, implementations in src/.' },
    { folder: 'internals/', location: 'Both', purpose: 'Private headers in include/, private implementations in src/. Never exposed outside the feature.' },
    { folder: '{feature}.h', location: 'include/{feature}/', purpose: 'The single public entry point for the feature. Consumers include only this file.' },
    { folder: '{feature}.c / {feature}.cpp', location: 'src/{feature}/', purpose: 'Feature initialisation or coordinator. Optional.' }
  ];

  readonly coreLayout: CoreLayoutEntry[] = [
    { item: 'src', content: 'Feature folders, main.c / main.cpp.', access: 'Read / write' },
    { item: 'include', content: 'Feature folders and their sub-folders.', access: 'Read / write' },
    { item: 'lib', content: 'All sub-folders; .c, .cpp, .h library files.', access: 'Read-only' },
    { item: 'platformio.ini', content: 'Project configuration file.', access: 'Read-only' }
  ];

  readonly espIdfLayout: EspIdfEntry[] = [
    { item: 'managed_components', content: 'All sub-folders; .c, .cpp, .h files.', access: 'Read-only' },
    { item: 'components', content: 'All sub-folders; .c, .cpp, .h files.', access: 'Read-only' },
    { item: 'sdkconfig*', content: 'ESP-IDF configuration files.', access: 'Read-only' }
  ];

  readonly steps: Step[] = [
    {
      number: 1,
      title: 'Create a PlatformIO project',
      description: 'In Visual Studio Code.'
    },
    {
      number: 2,
      title: 'Open Cyréna and start a New Chat',
      description: ''
    },
    {
      number: 3,
      title: 'Expand the Embedded shortcuts',
      description: ''
    },
    {
      number: 4,
      title: 'Click PlatformIO',
      description: ''
    },
    {
      number: 5,
      title: 'Configure the chat',
      description: 'Enter a title for the chat. Provide the full path to the platformio.ini file (or browse to select it). Choose the AI connection you wish to use. Optionally enable or disable specific Cyréna features.'
    },
    {
      number: 6,
      title: 'Press Submit',
      description: ''
    },
    {
      number: 7,
      title: 'Begin chatting with the AI',
      description: 'Add or modify source files. Implement new features using the enforced structure. Resolve build issues. Ask any other project-specific questions.'
    }
  ];
}
