import { ChangeDetectionStrategy, Component } from '@angular/core';

interface WorkflowStep {
  number: number;
  title: string;
  description: string;
}

interface AiapiField {
  field: string;
  type: string;
  required: boolean;
  description: string;
}

@Component({
  selector: 'app-api-references',
  standalone: true,
  imports: [],
  templateUrl: './api-references.component.html',
  styleUrl: './api-references.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ApiReferencesComponent {
  readonly workflowSteps: WorkflowStep[] = [
    {
      number: 1,
      title: 'Build Your Library',
      description: 'Develop your extension, library, or package with Cyréna. As you work, Cyréna automatically builds out API Reference documents — capturing service contracts, models, integration patterns, and architecture rules. Both you and Cyréna can add, edit, and remove references at any time to keep the documentation accurate and complete.'
    },
    {
      number: 2,
      title: 'Export as .aiapi',
      description: 'When your API surface is stable, export the API References from your chat as a .aiapi file. This is a JSON-formatted file with a distinctive extension that makes it easy to identify, share, and import.'
    },
    {
      number: 3,
      title: 'Ship with Your Package',
      description: 'Include the .aiapi file alongside your library — in the repository, in the NuGet package, or in documentation. Consumers of your library get the API documentation for free.'
    },
    {
      number: 4,
      title: 'Import in Any Chat',
      description: 'When a developer installs your library and opens it in Cyréna, they import the .aiapi file. The AI immediately understands the library\'s contracts, patterns, and usage — no manual training required.'
    }
  ];

  readonly aiapiFields: AiapiField[] = [
    { field: 'id', type: 'string (GUID)', required: true, description: 'Unique identifier for the API reference document' },
    { field: 'title', type: 'string', required: true, description: 'Human-readable title of the API reference' },
    { field: 'summary', type: 'string', required: true, description: 'Brief description of what the reference covers' },
    { field: 'content', type: 'string (markdown)', required: true, description: 'Full technical documentation with signatures, contracts, and usage patterns' },
    { field: 'keywords', type: 'string[]', required: true, description: 'Searchable tags for discovery and categorization' },
    { field: 'fileId', type: 'string', required: false, description: 'Optional link to a specific source file in the project' },
    { field: 'createdAt', type: 'ISO 8601 date', required: false, description: 'Timestamp when the reference was created' },
    { field: 'updatedAt', type: 'ISO 8601 date', required: false, description: 'Timestamp when the reference was last modified' }
  ];

  readonly exampleAiapi = `{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Cyrena.Tavily.Services.Internet",
  "summary": "Semantic Kernel plugin providing web search via Tavily API.",
  "content": "## Internet Plugin\\n\\nProvides AI-accessible web search...",
  "keywords": ["Tavily", "web search", "Semantic Kernel", "plugin"],
  "fileId": "services_ts_internet.service",
  "createdAt": "2025-01-15T10:30:00Z",
  "updatedAt": "2025-06-20T14:22:00Z"
}`;

  readonly benefits = [
    {
      icon: 'bi-share',
      title: 'Shippable Memory',
      description: 'API References travel with your code. Commit them to Git, include them in NuGet packages, or distribute them with your library.'
    },
    {
      icon: 'bi-lightning-charge',
      title: 'Zero-Setup Onboarding',
      description: 'New developers import the .aiapi file and the AI immediately knows how to use your library — no context building, no repeated explanations.'
    },
    {
      icon: 'bi-shield-check',
      title: 'Version-Controlled Docs',
      description: 'Because .aiapi files are plain JSON, they diff cleanly in Git. Track documentation changes alongside code changes.'
    },
    {
      icon: 'bi-people',
      title: 'Team Handoff',
      description: 'When a teammate clones the repo and opens it in Cyréna, the AI reads the same API References and maintains continuity.'
    }
  ];

  readonly proTip = `Ask Cyréna to maintain a single API Reference for this project that will be shipped along with your library. That way you only need to export 1 .aiapi file to document library updates — the AI keeps it current as the codebase evolves.`;

}
