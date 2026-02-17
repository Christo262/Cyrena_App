using Cyrena.Developer.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Cyrena.Developer.Options
{
    public class SolutionParser
    {
        /// <summary>
        /// Parses a solution file (.sln or .slnx) and returns the paths to all projects
        /// </summary>
        /// <param name="solutionFilePath">Path to the .sln or .slnx file</param>
        /// <returns>List of project file paths</returns>
        public static List<string> GetProjectPaths(string solutionFilePath)
        {
            if (!File.Exists(solutionFilePath))
            {
                throw new FileNotFoundException($"Solution file not found: {solutionFilePath}");
            }

            string extension = Path.GetExtension(solutionFilePath).ToLowerInvariant();

            return extension switch
            {
                ".sln" => ParseSlnFile(solutionFilePath),
                ".slnx" => ParseSlnxFile(solutionFilePath),
                _ => throw new ArgumentException($"Unsupported file type: {extension}. Expected .sln or .slnx")
            };
        }

        /// <summary>
        /// Parses a traditional .sln file
        /// </summary>
        private static List<string> ParseSlnFile(string slnFilePath)
        {
            var projectPaths = new List<string>();
            var slnDirectory = Path.GetDirectoryName(slnFilePath) ?? string.Empty;

            // Regex pattern to match project lines in .sln files
            // Format: Project("{GUID}") = "ProjectName", "RelativePath\Project.csproj", "{GUID}"
            var projectRegex = new Regex(
                @"Project\(""\{[A-F0-9-]+\}""\)\s*=\s*""[^""]+""\s*,\s*""([^""]+)""",
                RegexOptions.IgnoreCase
            );

            foreach (string line in File.ReadLines(slnFilePath))
            {
                var match = projectRegex.Match(line);
                if (match.Success)
                {
                    string relativePath = match.Groups[1].Value;

                    // Skip solution folders (they have .sln extension or specific GUIDs)
                    if (relativePath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Convert to absolute path
                    string absolutePath = Path.Combine(slnDirectory, relativePath);
                    absolutePath = Path.GetFullPath(absolutePath);

                    // Only add if it's a project file (common extensions)
                    if (IsProjectFile(relativePath))
                    {
                        projectPaths.Add(absolutePath);
                    }
                }
            }

            return projectPaths;
        }

        /// <summary>
        /// Parses a new XML-based .slnx file (Visual Studio 2022+)
        /// </summary>
        private static List<string> ParseSlnxFile(string slnxFilePath)
        {
            var projectPaths = new List<string>();
            var slnDirectory = Path.GetDirectoryName(slnxFilePath) ?? string.Empty;

            try
            {
                XDocument doc = XDocument.Load(slnxFilePath);

                // .slnx format uses <Project Path="..."> elements
                var projectElements = doc.Descendants("Project")
                    .Where(e => e.Attribute("Path") != null);

                foreach (var projectElement in projectElements)
                {
                    string? relativePath = projectElement.Attribute("Path")?.Value;

                    if (string.IsNullOrWhiteSpace(relativePath))
                        continue;

                    // Convert to absolute path
                    string absolutePath = Path.Combine(slnDirectory, relativePath);
                    absolutePath = Path.GetFullPath(absolutePath);

                    projectPaths.Add(absolutePath);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse .slnx file: {slnxFilePath}", ex);
            }

            return projectPaths;
        }

        /// <summary>
        /// Checks if a file is a valid project file based on extension
        /// </summary>
        private static bool IsProjectFile(string path)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();

            // Common project file extensions
            var projectExtensions = new HashSet<string>
            {
                ".csproj",    // C#
                ".vbproj",    // VB.NET
                ".fsproj",    // F#
                ".vcxproj",   // C++
                ".sqlproj",   // SQL Server
                ".sfproj",    // Service Fabric
                ".wapproj",   // Windows Application Packaging
                ".pyproj",    // Python
                ".njsproj",   // Node.js
                ".shproj",    // Shared Project
                ".esproj"     // JavaScript/TypeScript
            };

            return projectExtensions.Contains(extension);
        }

        /// <summary>
        /// Gets project paths and returns them with additional information
        /// </summary>
        public static List<ProjectInfo> GetProjectDetails(string solutionFilePath)
        {
            var projectPaths = GetProjectPaths(solutionFilePath);
            var projectDetails = new List<ProjectInfo>();

            foreach (var path in projectPaths)
            {
                projectDetails.Add(new ProjectInfo
                {
                    AbsolutePath = path,
                    FileName = Path.GetFileName(path),
                    ProjectName = Path.GetFileNameWithoutExtension(path),
                    ProjectType = Path.GetExtension(path),
                    Exists = File.Exists(path)
                });
            }

            return projectDetails;
        }
    }
}
