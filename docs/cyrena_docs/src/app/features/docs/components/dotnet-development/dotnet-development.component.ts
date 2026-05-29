import { ChangeDetectionStrategy, Component } from '@angular/core';

interface ProjectType {
  name: string;
  description: string;
}

interface FolderEntry {
  name: string;
  purpose: string;
  readOnly: boolean;
}

interface BlazorFolder {
  name: string;
  content: string;
  readOnly: boolean;
}

interface MvcFolder {
  name: string;
  content: string;
  readOnly: boolean;
}

interface Step {
  number: number;
  title: string;
  description: string;
}

@Component({
  selector: 'app-dotnet-development',
  standalone: true,
  imports: [],
  templateUrl: './dotnet-development.component.html',
  styleUrl: './dotnet-development.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DotnetDevelopmentComponent {
  readonly extensionId = 'cyrena.dotnet.csharp';

  readonly prerequisites = [
    '.NET SDK — installed and on your PATH',
    'IDE — Visual Studio, VS Code, Rider, or any editor that supports .NET projects',
    'Cyrena .NET Development extension (cyrena.dotnet.csharp) — installed and enabled in Cyrena'
  ];

  readonly projectTypes: ProjectType[] = [
    { name: 'Class Library', description: 'Standard reusable library (.csproj).' },
    { name: 'Console Application', description: 'Simple command-line program.' },
    { name: 'MVC Web App', description: 'ASP.NET Core Model-View-Controller web application.' },
    { name: 'MVC Library', description: 'Library that contains MVC-related components (controllers, views, etc.).' },
    { name: 'Blazor Component Library', description: 'Reusable UI components for Blazor.' },
    { name: 'Blazor Web Application', description: 'Full-stack Blazor client-side or server-side app.' }
  ];

  readonly baseFolders: FolderEntry[] = [
    { name: 'Attributes', purpose: 'Custom attributes for metadata and decoration.', readOnly: false },
    { name: 'Contracts', purpose: 'Dependency-injection interfaces.', readOnly: false },
    { name: 'Extensions', purpose: 'Static helper / extension classes.', readOnly: false },
    { name: 'Models', purpose: 'Data models and DTOs.', readOnly: false },
    { name: 'Services', purpose: 'Business logic services.', readOnly: false }
  ];

  readonly blazorFolders: BlazorFolder[] = [
    { name: 'Components', content: 'Reusable Razor components (.razor).', readOnly: false },
    { name: 'Pages', content: 'Razor page components for routing.', readOnly: false },
    { name: 'Layout', content: 'Main layout components.', readOnly: false },
    { name: 'wwwroot', content: 'Static web assets (CSS, JS, images).', readOnly: false }
  ];

  readonly mvcFolders: MvcFolder[] = [
    { name: 'Controllers', content: 'MVC controller classes.', readOnly: false },
    { name: 'Views', content: 'Razor view templates (.cshtml).', readOnly: false },
    { name: 'ViewModels', content: 'View-specific models.', readOnly: false },
    { name: 'wwwroot', content: 'Static web assets.', readOnly: false }
  ];

  readonly steps: Step[] = [
    {
      number: 1,
      title: 'Create the project',
      description: 'Use your IDE\'s UI or the dotnet CLI (dotnet new <template> …).'
    },
    {
      number: 2,
      title: 'Open Cyrena and start a New Chat',
      description: 'Launch the Cyréna desktop app and create a new chat session.'
    },
    {
      number: 3,
      title: 'Expand the ".NET Development" shortcuts',
      description: 'Look for the .NET Development section in the chat shortcuts panel.'
    },
    {
      number: 4,
      title: 'Choose the project type or solution',
      description: 'Select the appropriate project type or pick an existing solution.'
    },
    {
      number: 5,
      title: 'Configure the dialog',
      description: 'Provide the full path to the .csproj or .sln/.slnx file. Select your preferred AI connection and enable any additional model features you need.'
    },
    {
      number: 6,
      title: 'Submit and start chatting',
      description: 'Click Submit and begin chatting with the AI to generate, modify, or reason about your code.'
    }
  ];
}
