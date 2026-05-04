import { ChangeDetectionStrategy, Component } from '@angular/core';

interface Prerequisite {
  text: string;
}

interface GlobalFolder {
  name: string;
  purpose: string;
}

interface FeatureFolder {
  name: string;
  purpose: string;
}

interface TopLevelFolder {
  name: string;
  purpose: string;
}

interface Step {
  number: number;
  title: string;
  description: string;
}

@Component({
  selector: 'app-angular-development',
  standalone: true,
  imports: [],
  templateUrl: './angular-development.component.html',
  styleUrl: './angular-development.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AngularDevelopmentComponent {
  readonly extensionId = 'cyrena.angular';

  readonly prerequisites: Prerequisite[] = [
    { text: 'Node.js — installed and on your PATH.' },
    { text: 'Angular CLI — installed globally (npm install -g @angular/cli).' },
    { text: 'Cyréna Angular extension (cyrena.angular) — installed and enabled in Cyréna.' }
  ];

  readonly globalFolders: GlobalFolder[] = [
    { name: 'components/', purpose: 'Global reusable components.' },
    { name: 'services/', purpose: 'Global shared services.' },
    { name: 'guards/', purpose: 'Global route guards.' },
    { name: 'pipes/', purpose: 'Global custom pipes.' },
    { name: 'directives/', purpose: 'Global custom directives.' },
    { name: 'models/', purpose: 'Global shared models.' },
    { name: 'interceptors/', purpose: 'Global HTTP interceptors.' },
    { name: 'resolvers/', purpose: 'Global route resolvers.' },
    { name: 'features/', purpose: 'Feature modules (see below).' }
  ];

  readonly featureFolders: FeatureFolder[] = [
    { name: 'components/', purpose: 'Feature-scoped components.' },
    { name: 'services/', purpose: 'Feature-scoped services.' },
    { name: 'guards/', purpose: 'Feature-scoped route guards.' },
    { name: 'pipes/', purpose: 'Feature-scoped pipes.' },
    { name: 'directives/', purpose: 'Feature-scoped directives.' },
    { name: 'models/', purpose: 'Feature-scoped models.' },
    { name: 'interceptors/', purpose: 'Feature-scoped HTTP interceptors.' },
    { name: 'resolvers/', purpose: 'Feature-scoped route resolvers.' }
  ];

  readonly topLevelFolders: TopLevelFolder[] = [
    { name: 'src/assets/', purpose: 'Static assets.' },
    { name: 'src/styles/', purpose: 'Global stylesheets.' },
    { name: 'src/environments/', purpose: 'Environment configuration files.' },
    { name: 'public/', purpose: 'Angular v17+ static assets.' },
    { name: 'e2e/', purpose: 'End-to-end tests.' }
  ];

  readonly steps: Step[] = [
    {
      number: 1,
      title: 'Create the project',
      description: 'Use the Angular CLI: ng new my-app'
    },
    {
      number: 2,
      title: 'Open Cyréna and start a New Chat',
      description: 'Launch the Cyréna desktop app and create a new chat session.'
    },
    {
      number: 3,
      title: 'Expand the "Web Development" shortcuts',
      description: 'Look for the Web Development section in the chat shortcuts panel.'
    },
    {
      number: 4,
      title: 'Select Angular',
      description: 'Choose Angular from the available project types.'
    },
    {
      number: 5,
      title: 'Configure the dialog',
      description: 'Browse to your project\'s angular.json file to establish the project root. Enter a Title to identify this project in the sidebar. Select the AI Connection to use. Toggle any Activated Features you need.'
    },
    {
      number: 6,
      title: 'Submit and start chatting',
      description: 'Click Submit and begin chatting with the AI to generate, modify, or reason about your code.'
    }
  ];
}
