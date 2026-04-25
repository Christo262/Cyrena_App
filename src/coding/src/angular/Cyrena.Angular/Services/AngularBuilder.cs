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

## RIGID FOLDER STRUCTURE — THE AI DOES NOT CHOOSE PATHS

The Angular plugin enforces a FIXED structure. You do NOT decide where files go. You only provide:
1. The artifact name (e.g., `UserProfile`, `UserService`)
2. Optionally, a feature name (e.g., `users`, `admin`)

The plugin places the file in the EXACT correct location automatically.

### Structure

```
src/
  app/
    components/         # Global reusable components
    services/         # Global shared services
    guards/           # Global route guards
    pipes/            # Global custom pipes
    directives/       # Global custom directives
    models/           # Global shared models
    interceptors/     # Global HTTP interceptors
    resolvers/        # Global route resolvers
    features/         # Feature modules
      feature-name/
        components/     # Components specific to this feature
        services/       # Services specific to this feature
        guards/         # Guards specific to this feature
        pipes/          # Pipes specific to this feature
        directives/     # Directives specific to this feature
        models/         # Models specific to this feature
        interceptors/   # Interceptors specific to this feature
        resolvers/      # Resolvers specific to this feature
    app.component.ts
    app.config.ts
    app.routes.ts
  assets/             # Static assets
  styles/             # Global styles
  environments/       # Environment files
  index.html
  main.ts
public/               # Angular v17+ static assets
e2e/                  # End-to-end tests
```

### ABSOLUTE RULES

1. **You NEVER specify a folder path.** The plugin decides the path based on the artifact type and optional feature name.
2. **Components go in `components/` (global) or `features/<feature>/components/` (feature).**
3. **Services go in `services/` (global) or `features/<feature>/services/` (feature).**
4. **Guards go in `guards/` (global) or `features/<feature>/guards/` (feature).**
5. **Pipes go in `pipes/` (global) or `features/<feature>/pipes/` (feature).**
6. **Directives go in `directives/` (global) or `features/<feature>/directives/` (feature).**
7. **Models go in `models/` (global) or `features/<feature>/models/` (feature).**
8. **Interceptors go in `interceptors/` (global) or `features/<feature>/interceptors/` (feature).**
9. **Resolvers go in `resolvers/` (global) or `features/<feature>/resolvers/` (feature).**
10. **Global stylesheets go in `src/styles/`.**
11. **Environment files go in `src/environments/`.**
12. **Assets go in `src/assets/`.**
13. **e2e tests go in `e2e/`.**
14. **Public files go in `public/`.**
15. **NEVER create files outside these locations.**

### How to Create Artifacts

**Global artifact (no feature):**
```
create_component(name="UserProfile")
→ Creates: src/app/components/user-profile/user-profile.component.ts
           src/app/components/user-profile/user-profile.component.html
           src/app/components/user-profile/user-profile.component.css
           src/app/components/user-profile/user-profile.component.spec.ts

create_service(name="UserService")
→ Creates: src/app/services/user.service.ts
```

**Feature artifact:**
```
create_component(name="UserProfile", inFeature="users")
→ Creates: src/app/features/users/components/user-profile/user-profile.component.ts
           src/app/features/users/components/user-profile/user-profile.component.html
           src/app/components/user-profile/user-profile.component.css
           src/app/components/user-profile/user-profile.component.spec.ts

create_service(name="UserService", inFeature="users")
→ Creates: src/app/features/users/services/user.service.ts
```

**Create a feature first, then add artifacts to it:**
```
create_feature(name="users")
create_component(name="UserList", inFeature="users")
create_service(name="UserApiService", inFeature="users")
create_model(name="User", inFeature="users")
```

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
| Model | `*.model.ts` | `user.model.ts` |
| Interceptor | `*.interceptor.ts` | `auth.interceptor.ts` |
| Resolver | `*.resolver.ts` | `user.resolver.ts` |

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

## Available Tools

Use the following functions to scaffold Angular artifacts. You ONLY provide the name and optional feature. The plugin handles paths.

- `get_project_structure` — Lists all folders and files in the project
- `create_feature` — Creates a feature folder under src/app/features/ with standard subfolders
- `create_component` — Creates a standalone component. Pass `inFeature` to place in a feature, or omit for global.
- `create_service` — Creates an injectable service. Pass `inFeature` to place in a feature, or omit for global.
- `create_guard` — Creates a route guard. Pass `inFeature` to place in a feature, or omit for global.
- `create_pipe` — Creates a custom pipe. Pass `inFeature` to place in a feature, or omit for global.
- `create_directive` — Creates a custom directive. Pass `inFeature` to place in a feature, or omit for global.
- `create_model` — Creates a TypeScript model/interface. Pass `inFeature` to place in a feature, or omit for global.
- `create_interceptor` — Creates an HTTP interceptor. Pass `inFeature` to place in a feature, or omit for global.
- `create_resolver` — Creates a route resolver. Pass `inFeature` to place in a feature, or omit for global.
- `create_stylesheet` — Creates a global stylesheet in `src/styles/`
- `create_environment` — Creates an environment file in `src/environments/`
- `create_asset` — Creates an asset file in `src/assets/`
- `create_e2e` — Creates an e2e test file in `e2e/`
- `create_public_file` — Creates a public file in `public/`
- `build` — Runs `ng build` to verify compilation

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
11. After making changes, call `build` to verify the project compiles correctly
12. **NEVER try to specify a folder path. Use `inFeature` or omit it. The plugin decides the rest.**
13. **If an artifact belongs to a feature, ALWAYS pass the `inFeature` parameter**
14. **If an artifact is shared across features, NEVER pass `inFeature`**
""";
        }
    }
}
