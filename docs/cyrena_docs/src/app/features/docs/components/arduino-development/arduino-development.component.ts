import { ChangeDetectionStrategy, Component } from '@angular/core';

interface Step {
  number: number;
  title: string;
  description: string;
}

@Component({
  selector: 'app-arduino-development',
  standalone: true,
  imports: [],
  templateUrl: './arduino-development.component.html',
  styleUrl: './arduino-development.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ArduinoDevelopmentComponent {
  readonly extensionId = 'cyrena.arduino_ide';

  readonly prerequisites = [
    'Arduino IDE installed on the workstation',
    'Cyréna Arduino IDE extension (cyrena.arduino_ide) installed and enabled'
  ];

  readonly steps: Step[] = [
    {
      number: 1,
      title: 'Create or locate a sketch',
      description: 'Save a new .ino file or open an existing one.'
    },
    {
      number: 2,
      title: 'Open Cyréna and start a New Chat',
      description: 'Launch the Cyréna desktop app and create a new chat session.'
    },
    {
      number: 3,
      title: 'Expand the Embedded shortcuts',
      description: 'Look for the Embedded section in the chat shortcuts panel.'
    },
    {
      number: 4,
      title: 'Click Arduino IDE',
      description: 'Select the Arduino IDE option from the Embedded shortcuts.'
    },
    {
      number: 5,
      title: 'Configure the dialog',
      description: 'Provide the full path to the .ino sketch, give the chat a descriptive name, choose your AI connection, and enter the target board\'s name, RAM size, and clock speed. Enable any additional features you need.'
    },
    {
      number: 6,
      title: 'Submit and start chatting',
      description: 'Click Submit and begin interacting with the AI — request code reviews, bug fixes, or any other assistance related to the sketch.'
    }
  ];
}
