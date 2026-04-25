using BootstrapBlazor.Components;
using Cyrena.Angular.Components.Shared;
using Cyrena.Angular.Extensions;
using Cyrena.Angular.Options;
using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Coding.Options;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Angular.Services
{
    internal class AngularBuilder : ICodeBuilder
    {
        private readonly IKernelController _kernel;
        public AngularBuilder(IKernelController kernel)
        {
            _kernel = kernel;
        }

        public string Id => AngularOptions.BuilderId;

        public Task<DevelopPlan> ConfigureAsync(CyrenaKernelBuilder options)
        {
            var angularJsonPath = options.ChatConfiguration[AngularOptions.AngularJson]
                ?? throw new InvalidOperationException("angular.json not configured. Use the Configure dialog to select your angular.json file.");

            var rootDir = Path.GetDirectoryName(angularJsonPath)
                ?? throw new InvalidOperationException("Invalid angular.json path.");

            var plan = new DevelopPlan(rootDir);

            // Comprehensive Angular project indexing
            plan.IndexAngularDefaultPlan();

            // Register the Angular plugin
            options.Plugins.AddFromType<Angular>();

            // Add the Angular system prompt
            var prompt = GetSystemPrompt();
            options.GetFeatureOption<IPromptManager>().AddPrompt(0, prompt);

            return Task.FromResult(plan);
        }

        public Task DeleteAsync(ChatConfiguration config)
        {
            return Task.CompletedTask;
        }

        public async Task EditAsync(ChatConfiguration config, IServiceProvider services)
        {
            var dialog = services.GetRequiredService<DialogService>();
            var rf = await dialog.ShowModal<Configure>(new ResultDialogOption()
            {
                Title = "Angular",
                Size = Size.Medium,
                ComponentParameters = new()
                {
                    {nameof(Configure.Model), config }
                },
                ButtonYesText = "Save",
                ButtonNoText = "Cancel",
            });
            if (rf == DialogResult.Yes)
                await _kernel.UpdateAsync(config, true);
        }

        private static string GetSystemPrompt()
        {
            return """
# Angular Development Assistant

You are an expert Angular developer specializing in modern Angular (v17+) with standalone components, signals, and the latest best practices.

## Architecture Principles

- **Standalone Components**: Always use standalone components. Do NOT use NgModules.
- **Signals**: Use Angular signals (`signal()`, `computed()`, `effect()`) for state management instead of RxJS where appropriate.
- **Dependency Injection**: Use the `inject()` function for DI in components and services.
- **Control Flow**: Use the new built-in control flow (`@if`, `@for`, `@switch`) instead of structural directives (`*ngIf`, `*ngFor`).
- **Inputs/Outputs**: Use `input()` and `output()` signal-based inputs/outputs.
- **Change Detection**: Use `ChangeDetectionStrategy.OnPush` for all components.

## File Naming Conventions

| Type | Naming | Example |
|------|--------|---------|
| Component | `*.component.ts` | `user-profile.component.ts` |
| Template | `*.component.html` | `user-profile.component.html` |
| Styles | `*.component.css` or `*.component.scss` | `user-profile.component.scss` |
| Spec | `*.component.spec.ts` | `user-profile.component.spec.ts` |
| Service | `*.service.ts` | `user.service.ts` |
| Guard | `*.guard.ts` | `auth.guard.ts` |
| Pipe | `*.pipe.ts` | `currency.pipe.ts` |
| Directive | `*.directive.ts` | `highlight.directive.ts` |
| Model | `*.model.ts` or `*.interface.ts` | `user.model.ts` |

## Component Template

```typescript
import { Component, input, output, signal, computed, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserProfileComponent {
  // Signal inputs
  userId = input.required<string>();

  // Regular signals
  loading = signal(false);

  // Computed signals
  displayName = computed(() => `User: ${this.userId()}`);

  // Outputs
  save = output<void>();

  // Dependency injection
  private userService = inject(UserService);
}
```

## Service Template

```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);

  getUsers() {
    return this.http.get<User[]>('/api/users');
  }
}
```

## Guard Template

```typescript
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const isAuthenticated = /* check auth */;

  if (!isAuthenticated) {
    router.navigate(['/login']);
    return false;
  }
  return true;
};
```

## Pipe Template

```typescript
import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'myCustom',
  standalone: true
})
export class MyCustomPipe implements PipeTransform {
  transform(value: string, ...args: unknown[]): string {
    return value.toUpperCase();
  }
}
```

## Directive Template

```typescript
import { Directive, ElementRef, HostListener } from '@angular/core';

@Directive({
  selector: '[appHighlight]',
  standalone: true
})
export class HighlightDirective {
  constructor(private el: ElementRef) {}

  @HostListener('mouseenter') onMouseEnter() {
    this.el.nativeElement.style.backgroundColor = 'yellow';
  }
}
```

## Project Structure

```
src/
  app/
    components/       # Reusable UI components
    services/         # Injectable services
    guards/           # Route guards
    pipes/            # Custom pipes
    directives/       # Custom directives
    models/           # TypeScript interfaces/types
    features/         # Feature modules (lazy-loaded routes)
      feature-name/
        components/
        services/
        feature.routes.ts
    app.component.ts
    app.config.ts
    app.routes.ts
  assets/             # Static assets
  styles/             # Global styles
    variables.scss
    mixins.scss
    global.scss
  index.html
  main.ts
```

## Available Tools

Use the following functions to scaffold Angular artifacts:

- `get_project_structure` — Lists all components, services, guards, pipes, directives, and models
- `create_component` — Creates a standalone component with .ts, .html, .css, .spec.ts files
- `create_service` — Creates an injectable service
- `create_guard` — Creates a route guard
- `create_pipe` — Creates a custom pipe
- `create_directive` — Creates a custom directive
- `create_model` — Creates a TypeScript model/interface file
- `create_stylesheet` — Creates a global stylesheet (css, scss, less)
- `create_folder` — Creates a folder within src/app

## Rules

1. Always use kebab-case for file names and selectors
2. Always use PascalCase for class names
3. Always use camelCase for properties and methods
4. Prefer signals over RxJS for component state
5. Use `input()` and `output()` for component communication
6. Use `inject()` instead of constructor injection
7. Use standalone components — no NgModules
8. Use the new control flow syntax (`@if`, `@for`, `@switch`)
9. Add proper TypeScript types — avoid `any`
10. Write unit tests for all components and services
""";
        }
    }
}
