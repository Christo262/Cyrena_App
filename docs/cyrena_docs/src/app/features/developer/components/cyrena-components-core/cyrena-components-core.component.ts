import { ChangeDetectionStrategy, Component } from '@angular/core';

interface DisplayMethod {
  method: string;
  description: string;
}

interface LanguageMapping {
  extensions: string;
  language: string;
}

interface SettingsOverload {
  signature: string;
  description: string;
}

interface BuilderOverload {
  signature: string;
  description: string;
}

interface SharedComponentParam {
  param: string;
  description: string;
}

@Component({
  selector: 'app-cyrena-components-core',
  standalone: true,
  imports: [],
  templateUrl: './cyrena-components-core.component.html',
  styleUrl: './cyrena-components-core.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CyrenaComponentsCoreComponent {
  readonly namespaces = [
    'Cyrena.Contracts',
    'Cyrena.Models',
    'Cyrena.Options',
    'Cyrena.Extensions',
    'Cyrena.Components.Shared'
  ];

  readonly displayMethods: DisplayMethod[] = [
    { method: 'ShowModal<TComponent>(ResultDialogOption, Dialog?)', description: 'Shows a modal dialog with a typed Blazor component' },
    { method: 'ShowModal(string, string, ResultDialogOption?, Dialog?)', description: 'Shows a simple text modal dialog' },
    { method: 'ShowToast(ToastOption, ToastContainer?)', description: 'Displays a toast notification' },
    { method: 'ShowErrorToast(string?, string?, bool)', description: 'Convenience method for error toasts' },
    { method: 'ShowWarnToast(string?, string?, bool)', description: 'Convenience method for warning toasts' },
    { method: 'ShowSuccessToast(string?, string?, bool)', description: 'Convenience method for success toasts' },
    { method: 'ShowInfoToast(string?, string?, bool)', description: 'Convenience method for info toasts' },
    { method: 'NavigateTo(string)', description: 'Navigates to a URL in the Blazor router' }
  ];

  readonly languageMappings: LanguageMapping[] = [
    { extensions: '.c, .h', language: 'c' },
    { extensions: '.cpp, .hpp, .ino', language: 'cpp' },
    { extensions: '.cs', language: 'csharp' },
    { extensions: '.razor', language: 'html' },
    { extensions: '.css', language: 'css' },
    { extensions: '.js', language: 'javascript' },
    { extensions: '.md', language: 'markdown' },
    { extensions: '.csproj, .xml', language: 'xml' },
    { extensions: '.json', language: 'json' }
  ];

  readonly settingsOverloads: SettingsOverload[] = [
    { signature: 'AddSettingsComponent<TComponent>(ComponentOptions)', description: 'Obsolete — use section overloads instead' },
    { signature: 'AddSettingsComponent<TComponent>(ComponentOptions, string)', description: 'Registers component under a named section' },
    { signature: 'AddSettingsComponent<TComponent>(ComponentOptions, string, int)', description: 'Registers component under a section with display order' }
  ];

  readonly builderSettingsOverloads: BuilderOverload[] = [
    { signature: 'AddSettingsComponent<TComponent>(CyrenaBuilder, string)', description: 'Registers a settings component under a section' },
    { signature: 'AddSettingsComponent<TComponent>(CyrenaBuilder, string, int)', description: 'Registers with section and display order' },
    { signature: 'AddShortcut<TShortcut>(CyrenaBuilder)', description: 'Registers a shortcut as a scoped IShortcut service' }
  ];

  readonly codeInputParams: SharedComponentParam[] = [
    { param: 'Value / ValueChanged', description: 'Two-way bound editor content' },
    { param: 'Language', description: 'Monaco language mode (default: "plaintext")' }
  ];

  readonly connectionSelectorParams: SharedComponentParam[] = [
    { param: 'Value / ValueChanged', description: 'Two-way bound selected connection ID' },
    { param: 'Label', description: 'Dropdown label (default: "AI Connection")' }
  ];

  readonly pluginSelectorParams: SharedComponentParam[] = [
    { param: 'Chat (ChatConfiguration)', description: 'Chat whose PluginIds will be updated (required)' }
  ];
}
