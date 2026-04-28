import { ChangeDetectionStrategy, Component } from '@angular/core';

interface Step {
  number: number;
  title: string;
  description: string;
  code?: string;
  codeLang?: string;
}

interface BuilderMethod {
  method: string;
  description: string;
}

interface PluginMember {
  member: string;
  type: string;
  description: string;
}

interface ChecklistItem {
  file: string;
  description: string;
}

@Component({
  selector: 'app-extension-development',
  standalone: true,
  imports: [],
  templateUrl: './extension-development.component.html',
  styleUrl: './extension-development.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExtensionDevelopmentComponent {
  readonly extensionId = 'cyrena.extensa';

  readonly prerequisites = [
    '.NET SDK 10.0 or later',
    'Visual Studio, VS Code, or Rider',
    'Cyrena desktop application installed',
    'Basic familiarity with C# and dependency injection'
  ];

  readonly steps: Step[] = [
    {
      number: 1,
      title: 'Create a Razor Class Library',
      description: 'Start with a new Razor Class Library project. This is the foundation of every Cyréna extension. The Razor SDK is required if your extension contains Blazor components.'
    },
    {
      number: 2,
      title: 'Add Required Package References',
      description: 'Reference the core Cyréna packages that provide the extension infrastructure.',
      code: '<PackageReference Include="Cyrena.Extensa.Core" />\n<PackageReference Include="Cyrena.Components.Core" />\n<PackageReference Include="Microsoft.AspNetCore.Components.Web" />',
      codeLang: 'xml'
    },
    {
      number: 3,
      title: 'Create the Extension Entry Point',
      description: 'Implement the Extension base class. This is the single point of discovery for the Extensa loader.'
    },
    {
      number: 4,
      title: 'Add Builder Extension Methods',
      description: 'Create a static extension method on CyrenaBuilder. This is where you register plugins, components, and services.'
    },
    {
      number: 5,
      title: 'Implement IAssistantPlugin',
      description: 'This is where AI capabilities are added. Register Semantic Kernel plugins, add system prompts, and configure the kernel per-chat.'
    },
    {
      number: 6,
      title: 'Add Optional UI Components',
      description: 'If your extension needs settings panels or other UI, create Blazor components and register them via AddSettingsComponent<T>().'
    },
    {
      number: 7,
      title: 'Create the Extension Manifest',
      description: 'For runtime-loaded extensions, add an extension.json file with metadata, version, and dependencies.'
    },
    {
      number: 8,
      title: 'Build and Deploy',
      description: 'Compile your extension and either reference it at compile-time or package it as a ZIP for runtime loading via Extensa.'
    }
  ];

  readonly builderMethods: BuilderMethod[] = [
    { method: 'AddAssistantPlugin<T>()', description: 'Registers an IAssistantPlugin implementation' },
    { method: 'AddAssistantMode<T>()', description: 'Registers an IAssistantMode implementation' },
    { method: 'AddSettingsComponent<T>()', description: 'Registers a settings UI component' },
    { method: 'AddConnectionProvider<T>()', description: 'Registers an LLM connection provider' },
    { method: 'AddStore<T>()', description: 'Registers a persistence store' }
  ];

  readonly pluginMembers: PluginMember[] = [
    { member: 'Priority', type: 'int', description: 'Load order — lower values load earlier. Default is 0.' },
    { member: 'Modes', type: 'string[]', description: 'Compatible mode IDs. Empty array = compatible with ALL modes.' },
    { member: 'Id', type: 'string', description: 'Unique plugin identifier (reverse-domain recommended).' },
    { member: 'Required', type: 'bool', description: 'If true, the plugin cannot be disabled by the user.' },
    { member: 'Title', type: 'string', description: 'Human-readable display name.' },
    { member: 'LoadAsync(CyrenaKernelBuilder)', type: 'Task', description: 'Called per-chat when the kernel is initialized.' }
  ];

  readonly checklist: ChecklistItem[] = [
    { file: 'extension.json', description: 'Manifest with id, version, entryAssemblyFile' },
    { file: 'MyExtension.cs', description: 'Entry point class inheriting Extension' },
    { file: 'Extensions/CyrenaBuilderExtensions.cs', description: 'Builder extension method' },
    { file: 'Services/MyPlugin.cs', description: 'IAssistantPlugin implementation' },
    { file: 'Options/MyOptions.cs', description: 'Settings/options class with Key constant' },
    { file: 'Models/', description: 'Request/response models inheriting JsonStringObject' },
    { file: 'Components/Shared/MySettings.razor', description: 'Settings UI (optional)' },
    { file: 'Resources/', description: 'Embedded prompts, templates (optional)' },
    { file: '.csproj', description: 'Razor SDK, project references, embedded resources' }
  ];
}