using Cyrena.Developer.Models;

namespace Cyrena.Developer.Extensions
{
    public static class DevelopPlanExtensions
    {
        public static void IndexDefaultCSharpProject(this DevelopPlan plan)
        {
            var atts = plan.GetOrCreateFolder("attributes", "Attributes");
            plan.IndexFiles(atts, "cs", "attributes_");

            var contracts = plan.GetOrCreateFolder("contracts", "Contracts");
            plan.IndexFiles(contracts, "cs", "contracts_");

            var extensions = plan.GetOrCreateFolder("extensions", "Extensions");
            plan.IndexFiles(extensions, "cs", "extensions_");

            var models = plan.GetOrCreateFolder("models", "Models");
            plan.IndexFiles(models, "cs", "models_");

            var options = plan.GetOrCreateFolder("options", "Options");
            plan.IndexFiles(options, "cs", "options_");

            var services = plan.GetOrCreateFolder("services", "Services");
            plan.IndexFiles(services, "cs", "services_");

            plan.IndexFiles("csproj", "csproj_", true);
        }

        public static void IndexBlazorProjectType(this DevelopPlan plan)
        {
            var components = plan.GetOrCreateFolder("components", "Components");
            plan.IndexFiles(components, "razor", "blazor_root_");
            plan.IndexFiles(components, "cs", "blazor_root_cs_");
            plan.IndexFiles(components, "css", "blazor_root_css_");

            var pages = plan.GetOrCreateFolder(components, "pages", "Pages");
            plan.IndexFiles(pages, "razor", "blazor_pages_");
            plan.IndexFiles(pages, "cs", "blazor_pages_cs_");
            plan.IndexFiles(pages, "css", "blazor_pages_css_");

            var layout = plan.GetOrCreateFolder(components, "layouts", "Layout");
            plan.IndexFiles(layout, "razor", "blazor_layout_");
            plan.IndexFiles(layout, "cs", "blazor_layout_cs_");
            plan.IndexFiles(layout, "css", "blazor_layout_css_");

            var shared = plan.GetOrCreateFolder(components, "shared", "Shared");
            plan.IndexFiles(layout, "razor", "blazor_shared_");
            plan.IndexFiles(layout, "cs", "blazor_shared_cs_");
            plan.IndexFiles(layout, "css", "blazor_shared_css_");

            plan.IndexFiles("cs", "blazor_cs_");
            plan.IndexFiles("json", "blazor_json_");

            plan.IndexWwwroot();
        }

        public static void IndexMvcProjectType(this DevelopPlan plan)
        {
            var controllers = plan.GetOrCreateFolder("controllers", "Controllers");
            plan.IndexFiles(controllers, "cs", "controllers_");

            var views = plan.GetOrCreateFolder("views", "Views");
            plan.IndexFiles(views, "cshtml", "views_");

            var dirs = Directory.GetDirectories(Path.Combine(plan.RootDirectory, views.RelativePath));
            foreach (var dir in dirs)
            {
                var info = new DirectoryInfo(dir);
                var t_dir = plan.GetOrCreateFolder(views, $"views_{info.Name.ToLower()}", info.Name);
                plan.IndexFiles(t_dir, "cshtml", $"views_{info.Name.ToLower()}_");
            }

            plan.IndexFiles("cs", "mvc_cs_");
            plan.IndexFiles("json", "mvc_json_");

            plan.IndexWwwroot();
        }

        private static void IndexWwwroot(this DevelopPlan plan)
        {
            var wwwroot = plan.GetOrCreateFolder("public", "wwwroot");
            plan.IndexFiles("html", "public_html_");

            var css = plan.GetOrCreateFolder(wwwroot, "stylesheets", "css");
            plan.IndexFiles(css, "css", "public_stylesheet_");

            var js = plan.GetOrCreateFolder(wwwroot, "scripts", "js");
            plan.IndexFiles(js, "js", "public_script_");
        }
    }
}
