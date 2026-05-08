using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.Website.Services
{
    internal class WebsiteKernelFunctions
    {
        private readonly IChatMessageService _context;
        private readonly IDevelopPlanService _plan;

        public WebsiteKernelFunctions(IChatMessageService context, IDevelopPlanService plan)
        {
            _context = context;
            _plan = plan;
        }

        [KernelFunction("create_html")]
        [Description("Create a new HTML file in the project root.")]
        public ToolResult<DevelopFile> CreateHtml(
            [Description("The file name without extension. Will be appended with .html")] string name)
        {
            var fileName = $"{name}.html";
            var fileId = $"html_{name}";
            var file = _plan.Plan.CreateFile(fileId, fileName, GetHtmlBoilerplate(name));
            _plan.InvokeFileCreated(file);
            return new ToolResult<DevelopFile>(file);
        }

        [KernelFunction("create_css")]
        [Description("Create a new CSS file in the css/ folder.")]
        public ToolResult<DevelopFile> CreateCss(
            [Description("The file name without extension. Will be appended with .css")] string name)
        {
            var folder = _plan.Plan.GetOrCreateFolder("css", "css");
            var fileName = $"{name}.css";
            var fileId = $"css_{name}";
            var file = _plan.Plan.CreateFile(folder, fileId, fileName, GetCssBoilerplate(name));
            _plan.InvokeFileCreated(file);
            return new ToolResult<DevelopFile>(file);
        }

        [KernelFunction("create_js")]
        [Description("Create a new JavaScript file in the js/ folder.")]
        public ToolResult<DevelopFile> CreateJs(
            [Description("The file name without extension. Will be appended with .js")] string name)
        {
            var folder = _plan.Plan.GetOrCreateFolder("js", "js");
            var fileName = $"{name}.js";
            var fileId = $"js_{name}";
            var file = _plan.Plan.CreateFile(folder, fileId, fileName, GetJsBoilerplate(name));
            _plan.InvokeFileCreated(file);
            return new ToolResult<DevelopFile>(file);
        }

        [KernelFunction("create_image_folder")]
        [Description("Create a subfolder in the images/ directory for organizing image assets.")]
        public ToolResult<DevelopFolder> CreateImageFolder(
            [Description("The name of the image subfolder (e.g., 'icons', 'photos', 'logos')")] string name)
        {
            var images = _plan.Plan.GetOrCreateFolder("images", "images");
            var folder = _plan.Plan.CreateFolder(images, $"images_{name}", name);
            return new ToolResult<DevelopFolder>(folder);
        }

        [KernelFunction("create_asset")]
        [Description("Create an asset file in the assets/ folder. Use for .webmanifest, .txt, .json config files, etc.")]
        public ToolResult<DevelopFile> CreateAsset(
            [Description("The file name with extension (e.g., 'site.webmanifest', 'robots.txt')")] string name)
        {
            var folder = _plan.Plan.GetOrCreateFolder("assets", "assets");
            var baseName = Path.GetFileNameWithoutExtension(name);
            var fileId = $"asset_{baseName}";
            var file = _plan.Plan.CreateFile(folder, fileId, name, string.Empty);
            _plan.InvokeFileCreated(file);
            return new ToolResult<DevelopFile>(file);
        }

        private static string GetHtmlBoilerplate(string title)
        {
            return $$"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <title>{{title}}</title>
                    <link rel="stylesheet" href="css/styles.css">
                </head>
                <body>
                    <header>
                        <h1>{{title}}</h1>
                    </header>
                    <main>
                        <!-- Content goes here -->
                    </main>
                    <footer>
                        <p>&copy; {{DateTime.Now.Year}} {{title}}</p>
                    </footer>
                    <script src="js/scripts.js" defer></script>
                </body>
                </html>
                """;
        }

        private static string GetCssBoilerplate(string name)
        {
            return $$"""
                /* {{name}}.css */

                :root {
                    --primary-color: #2563eb;
                    --secondary-color: #64748b;
                    --background-color: #ffffff;
                    --text-color: #1e293b;
                    --font-family: system-ui, -apple-system, sans-serif;
                    --max-width: 1200px;
                    --spacing: 1rem;
                }

                *, *::before, *::after {
                    box-sizing: border-box;
                    margin: 0;
                    padding: 0;
                }

                body {
                    font-family: var(--font-family);
                    background-color: var(--background-color);
                    color: var(--text-color);
                    line-height: 1.6;
                }

                /* Add your styles below */
                """;
        }

        private static string GetJsBoilerplate(string name)
        {
            return $$"""
                // {{name}}.js

                document.addEventListener('DOMContentLoaded', () => {
                    // Initialize your application here
                    console.log('{{name}} loaded');
                });
                """;
        }
    }
}
