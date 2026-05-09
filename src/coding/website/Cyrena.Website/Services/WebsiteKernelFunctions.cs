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

        private static string[] _allowedTypes = [".html", ".css", ".js", ".xml", ".json", ".txt", ".webmanifest"];

        [KernelFunction("create_file")]
        [Description("Create a new file.")]
        public ToolResult<DevelopFile> CreateFile(
            [Description("The file name to create. For example: 'index.html', 'custom-styles.css'. You may create .html, .css, .js, .txt, .xml or .json files.")] string file)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var ext = Path.GetExtension(file);
            if (!_allowedTypes.Contains(ext))
                return new ToolResult<DevelopFile>(false, $"{ext} is not an allowed filetype you can create");
            _context.LogInfo($"Creating new file {file}");
            switch (ext)
            {
                case "css":
                    {
                        var css = _plan.Plan.GetOrCreateFolder("css", "css");
                        var f = _plan.Plan.CreateFile(css, $"css_{name}", file, GetCssBoilerplate(file));
                        return new ToolResult<DevelopFile>(f);
                    }
                case "js":
                    {
                        var js = _plan.Plan.GetOrCreateFolder("js", "js");
                        var f = (_plan.Plan.CreateFile(js,$"js_{name}",file, GetJsBoilerplate(file)));
                        return new ToolResult<DevelopFile>(f);
                    }
                case "html":
                    {
                        var f = _plan.Plan.CreateFile($"html_{name}", file, GetHtmlBoilerplate(file));
                        return new ToolResult<DevelopFile>(f);
                    }
                default:
                    {
                        var f = _plan.Plan.CreateFile($"{ext}_{name}", file, null);
                        return new ToolResult<DevelopFile>(f);
                    }
            }
        }

        [KernelFunction("create_asset")]
        [Description("Create an asset file in the assets/ folder. Use for .webmanifest, .txt, .json config files, etc.")]
        public ToolResult<DevelopFile> CreateAsset(
            [Description("The file name with extension (e.g., 'site.webmanifest', 'robots.txt')")] string name)
        {
            var folder = _plan.Plan.GetOrCreateFolder("assets", "assets");
            var baseName = Path.GetFileNameWithoutExtension(name);
            var ext = Path.GetExtension(name);
            if (!_allowedTypes.Contains(ext))
                return new ToolResult<DevelopFile>(false, $"{ext} is not an allowed filetype you can create");
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
