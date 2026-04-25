using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Extensions;
using Cyrena.Models;

namespace Cyrena.Angular.Extensions
{
    public static class ProjectPlanExtensions
    {
        /// <summary>
        /// Indexes a comprehensive Angular project DevelopPlan.
        /// Discovers the src/ tree dynamically and indexes all relevant file types.
        /// </summary>
        public static void IndexAngularDefaultPlan(this DevelopPlan plan)
        {
            // ── Root configuration files (read-only) ──────────────────────────
            plan.IndexFiles("json", "json_", true);   // angular.json, package.json, tsconfig*.json
            plan.IndexFiles("md", "md_", true);       // README.md
            plan.IndexFiles("editorconfig", "editorconfig_", true);
            plan.IndexFiles("gitignore", "gitignore_", true);
            plan.IndexFiles("js", "js_", true);       // karma.conf.js, etc.

            // ── src/ folder ───────────────────────────────────────────────────
            var src = plan.GetOrCreateFolder("src", "src");
            plan.IndexFiles(src, "ts", "src_ts_");
            plan.IndexFiles(src, "html", "src_html_");
            plan.IndexFiles(src, "css", "src_css_");
            plan.IndexFiles(src, "scss", "src_scss_");
            plan.IndexFiles(src, "less", "src_less_");

            // ── src/app/ and its subdirectories ───────────────────────────────
            var app = plan.GetOrCreateFolder(src, "app", "app");
            plan.IndexFiles(app, "ts", "app_ts_");
            plan.IndexFiles(app, "html", "app_html_");
            plan.IndexFiles(app, "css", "app_css_");
            plan.IndexFiles(app, "scss", "app_scss_");
            plan.IndexFiles(app, "less", "app_less_");

            // Dynamically discover subdirectories under src/app/
            var appPath = Path.Combine(plan.RootDirectory, app.RelativePath);
            if (Directory.Exists(appPath))
            {
                foreach (var dir in Directory.GetDirectories(appPath))
                {
                    var dirInfo = new DirectoryInfo(dir);
                    var folder = plan.GetOrCreateFolder(app, dirInfo.Name, dirInfo.Name);
                    IndexAngularFolder(plan, folder);
                }
            }

            // ── src/assets/ ───────────────────────────────────────────────────
            var assets = plan.GetOrCreateFolder(src, "assets", "assets");
            plan.IndexFiles(assets, "json", "assets_json_");
            plan.IndexFiles(assets, "svg", "assets_svg_");
            plan.IndexFiles(assets, "txt", "assets_txt_");

            // ── src/environments/ ─────────────────────────────────────────────
            var environments = plan.GetOrCreateFolder(src, "environments", "environments");
            plan.IndexFiles(environments, "ts", "env_ts_");

            // ── src/styles/ ───────────────────────────────────────────────────
            var styles = plan.GetOrCreateFolder(src, "styles", "styles");
            plan.IndexFiles(styles, "css", "styles_css_");
            plan.IndexFiles(styles, "scss", "styles_scss_");
            plan.IndexFiles(styles, "less", "styles_less_");

            // ── e2e/ (end-to-end tests) ─────────────────────────────────────
            var e2e = plan.GetOrCreateFolder("e2e", "e2e");
            plan.IndexFiles(e2e, "ts", "e2e_ts_");
            plan.IndexFiles(e2e, "js", "e2e_js_");
            plan.IndexFiles(e2e, "json", "e2e_json_");
            plan.IndexFiles(e2e, "html", "e2e_html_");

            // ── public/ (Angular v17+ static assets) ──────────────────────────
            var pub = plan.GetOrCreateFolder("public", "public");
            plan.IndexFiles(pub, "json", "public_json_");
            plan.IndexFiles(pub, "svg", "public_svg_");
            plan.IndexFiles(pub, "txt", "public_txt_");
        }

        /// <summary>
        /// Recursively indexes an Angular folder and its subdirectories.
        /// Handles nested feature folders (e.g., features/feature-name/components).
        /// </summary>
        private static void IndexAngularFolder(DevelopPlan plan, DevelopFolder folder)
        {
            var folderPath = Path.Combine(plan.RootDirectory, folder.RelativePath);
            if (!Directory.Exists(folderPath))
                return;

            // Index all relevant file types in this folder
            plan.IndexFiles(folder, "ts", $"{folder.Name}_ts_");
            plan.IndexFiles(folder, "html", $"{folder.Name}_html_");
            plan.IndexFiles(folder, "css", $"{folder.Name}_css_");
            plan.IndexFiles(folder, "scss", $"{folder.Name}_scss_");
            plan.IndexFiles(folder, "less", $"{folder.Name}_less_");
            plan.IndexFiles(folder, "json", $"{folder.Name}_json_", true);

            // Recurse into subdirectories
            foreach (var dir in Directory.GetDirectories(folderPath))
            {
                var dirInfo = new DirectoryInfo(dir);
                var subFolder = plan.GetOrCreateFolder(folder, dirInfo.Name, dirInfo.Name);
                IndexAngularFolder(plan, subFolder);
            }
        }
    }
}
