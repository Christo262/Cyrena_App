using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;

namespace Cyrena.Blazor.Extensions
{
    public static class ProjectPlanExtensions
    {
        public static ProjectPlan LoadBlazorDefaultPlan(this IDeveloperContextBuilder builder)
        {
            ProjectPlan.TryLoadFromDirectory(builder.Project.RootDirectory, out var plan);
            plan.IndexFiles("json", "app_json_");
            plan.IndexFiles("cs", "app_");
            plan.IndexFiles("md", "app_doc_");

            plan.IndexWwwroot();
            plan.IndexComponents();

            var extensions = plan.GetOrCreateFolder("extensions", "Extensions");
            plan.IndexFiles(extensions, "cs", "extensions_");

            var contracts = plan.GetOrCreateFolder("contracts", "Contracts");
            plan.IndexFiles(contracts, "cs", "contracts_");

            var models = plan.GetOrCreateFolder("models", "Models");
            plan.IndexFiles(models, "cs", "models_");

            var services = plan.GetOrCreateFolder("services", "Services");
            plan.IndexFiles(services, "cs", "services_");

            var options = plan.GetOrCreateFolder("options", "Options");
            plan.IndexFiles(options, "cs", "options_");

            ProjectPlan.Save(plan);
            return plan;
        }

        public static void RefreshBlazorProject(this ProjectPlan plan)
        {
            plan.IndexFiles("json", "app_json_");
            plan.IndexFiles("cs", "app_");
            plan.IndexFiles("md", "app_doc_");

            plan.IndexWwwroot();
            plan.IndexComponents();

            var extensions = plan.GetOrCreateFolder("extensions", "Extensions");
            plan.IndexFiles(extensions, "cs", "extensions_");

            var contracts = plan.GetOrCreateFolder("contracts", "Contracts");
            plan.IndexFiles(contracts, "cs", "contracts_");

            var models = plan.GetOrCreateFolder("models", "Models");
            plan.IndexFiles(models, "cs", "models_");

            var services = plan.GetOrCreateFolder("services", "Services");
            plan.IndexFiles(services, "cs", "services_");

            ProjectPlan.Save(plan);
        }

        public static void IndexComponents(this ProjectPlan plan)
        {
            var cmp = plan.GetOrCreateFolder("components", "Components");
            plan.IndexFiles(cmp, "razor", "components_");
            plan.IndexFiles(cmp, "css", "components_css_");
            plan.IndexFiles(cmp, "cs", "components_cs_");

            var layouts = plan.GetOrCreateFolder(cmp, "components_layout", "Layout");
            plan.IndexFiles(layouts, "razor", "components_layout_");
            plan.IndexFiles(layouts, "css", "components_layout_css_");
            plan.IndexFiles(layouts, "cs", "components_layout_cs_");

            var pages = plan.GetOrCreateFolder(cmp, "components_pages", "Pages");
            plan.IndexFiles(pages, "razor", "components_pages_");
            plan.IndexFiles(pages, "css", "components_pages_css_");
            plan.IndexFiles(pages, "cs", "components_pages_cs_");

            var shared = plan.GetOrCreateFolder(cmp, "components_shared", "Shared");
            plan.IndexFiles(shared, "razor", "components_shared_");
            plan.IndexFiles(shared, "css", "components_shared_css_");
            plan.IndexFiles(shared, "cs", "components_shared_cs_");
        }

        public static void IndexWwwroot(this ProjectPlan plan)
        {
            var www = plan.GetOrCreateFolder("wwwroot", "wwwroot");
            var style = plan.GetOrCreateFolder(www, "wwwroot_css", "css");
            plan.IndexFiles(style, "css", "styles_");
            var scripts = plan.GetOrCreateFolder(www, "scripts", "js");
            plan.IndexFiles(scripts, "js", "script_");
        }
    }
}
