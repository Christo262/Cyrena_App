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

  // Code examples stored as strings to avoid Angular ICU parsing issues with curly braces
  readonly idisplayServiceCode = `public interface IDisplayService
{
    Task<DialogResult> ShowModal<TComponent>(ResultDialogOption option, Dialog? dialog = null)
        where TComponent : IComponent, IResultDialog;

    Task<DialogResult> ShowModal(string title, string content, ResultDialogOption? option = null, Dialog? dialog = null);

    Task ShowToast(ToastOption option, ToastContainer? toastContainer = null);
    Task ShowErrorToast(string? title = null, string? content = null, bool autoHide = true);
    Task ShowWarnToast(string? title = null, string? content = null, bool autoHide = true);
    Task ShowSuccessToast(string? title = null, string? content = null, bool autoHide = true);
    Task ShowInfoToast(string? title = null, string? content = null, bool autoHide = true);

    void NavigateTo(string url);
}`;

  readonly ishortcutCode = `public interface IShortcut
{
    string Title { get; }
    string Description { get; }
    string Icon { get; }
    string Color { get; }
    string Category { get; }
    string[] Tags { get; }
    Task OnClick();
}`;

  readonly itoolbarComponentCode = `public interface IToolbarComponent
{
    Type Component { get; }
    ToolbarAlignment Alignment { get; }
}

public enum ToolbarAlignment
{
    Start, End
}`;

  readonly iviewStartProviderCode = `public interface IViewStartProvider
{
    IEnumerable<ViewStart> Provide();
}`;

  readonly kernelComponentBaseCode = `public abstract class KernelComponentBase : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public Kernel Kernel { get; set; } = default!;
}`;

  readonly viewStartCode = `public sealed class ViewStart
{
    public string Href { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
}`;

  readonly kernelInjectAttributeCode = `[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class KernelInjectAttribute : Attribute
{
    public object? Key { get; init; }
}`;

  readonly kernelInjectUsageCode = `public class MyToolbarComponent : KernelComponentBase
{
    [KernelInject]
    public IChatMessageService ChatService { get; set; } = default!;

    [KernelInject(Key = "my-key")]
    public IMyService MyService { get; set; } = default!;
}`;

  readonly componentOptionsCode = `public class ComponentOptions
{
    internal List<ComponentMetaData> SettingsComponents { get; set; }
    public ComponentMetaData[] GetSettingsComponents();
}

public record ComponentMetaData(Type Component, string? Section, int Order);`;

  readonly componentOptionsExtensionsCode = `public static class ComponentOptionsExtensions
{
    [Obsolete("Use new section mapping API")]
    public static void AddSettingsComponent<TComponent>(this ComponentOptions options)
        where TComponent : ComponentBase;

    public static void AddSettingsComponent<TComponent>(this ComponentOptions options, string section);

    public static void AddSettingsComponent<TComponent>(this ComponentOptions options, string section, int order);
}`;

  readonly codeLanguagesCode = `public class CodeLanguages
{
    public string GetFileLanguage(string extension);
}`;

  readonly cyrenaBuilderExtensionsCode = `public static class CyrenaBuilderExtensions
{
    [Obsolete("Use new section mapping API")]
    public static CyrenaBuilder AddSettingsComponent<TComponent>(this CyrenaBuilder builder)
        where TComponent : ComponentBase;

    public static CyrenaBuilder AddSettingsComponent<TComponent>(this CyrenaBuilder builder, string section)
        where TComponent : ComponentBase;

    public static CyrenaBuilder AddSettingsComponent<TComponent>(this CyrenaBuilder builder, string section, int order)
        where TComponent : ComponentBase;

    public static CyrenaBuilder AddShortcut<TShortcut>(this CyrenaBuilder builder)
        where TShortcut : class, IShortcut;
}`;

  readonly cyrenaKernelBuilderExtensionsCode = `public static class CyrenaKernelBuilderExtensions
{
    public static void AddToolbarComponent<TComponent>(this CyrenaKernelBuilder builder, ToolbarAlignment alignment)
        where TComponent : KernelComponentBase;

    [Obsolete]
    public static void AddToolbarComponent<TComponent>(this IKernelBuilder builder, ToolbarAlignment alignment)
        where TComponent : KernelComponentBase;
}`;

  readonly componentBaseExtensionsCode = `public static class ComponentBaseExtensions
{
    public static RenderFragment Render(this ComponentBase cmp, Type type);
    public static RenderFragment Render(this ComponentBase cmp, Type type, Dictionary<string, object?> parameters);
}`;

  readonly toolbarExampleCode = `public class MyToolbarComponent : KernelComponentBase
{
    [KernelInject]
    public IChatMessageService ChatService { get; set; } = default!;
}

// In IAssistantPlugin.LoadAsync:
builder.AddToolbarComponent<MyToolbarComponent>(ToolbarAlignment.End);`;

  readonly shortcutExampleCode = `public class MyShortcut : IShortcut
{
    public string Title => "My Action";
    public string Description => "Does something";
    public string Icon => "fa-solid fa-star";
    public string Color => "primary";
    public string Category => "My Category";
    public string[] Tags => ["my"];
    public Task OnClick() { ... }
}

// In extension BuildExtension:
builder.AddShortcut<MyShortcut>();`;

  readonly settingsExampleCode = `builder.AddSettingsComponent<MySettingsComponent>("General", 1);`;

  readonly viewStartExampleCode = `public class MyViewStartProvider : IViewStartProvider
{
    public IEnumerable<ViewStart> Provide()
    {
        yield return new ViewStart
        {
            Href = "/my-page",
            Title = "My Page"
        };
    }
}`;
}
