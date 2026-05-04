using Cyrena.Coding.Models;
using Cyrena.Coding.Extensions;
using Cyrena.Extensions;
using Cyrena.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Cyrena.PlatformIO.Extensions
{
    public static class ProjectPlanExtensions
    {
        public static void IndexPlatformIODefaultPlan(this DevelopPlan plan)
        {
            // ===== include/ =====
            var include = plan.GetOrCreateFolder("include", "include");

            // Index .h files directly in include/ root (writable)
            plan.IndexFiles(include, "h", "include_h_");

            // Index feature folders in include/
            var includeDirs = Directory.GetDirectories(Path.Combine(plan.RootDirectory, include.RelativePath));
            foreach (var dir in includeDirs)
            {
                var dirInfo = new DirectoryInfo(dir);
                var featureName = dirInfo.Name.ToLowerInvariant();
                var featureFolder = plan.GetOrCreateFolder(include, $"include_{featureName}", dirInfo.Name);

                // Ensure structured sub-folders exist: definitions, actions, internals
                var definitions = plan.GetOrCreateFolder(featureFolder, $"include_{featureName}_definitions", "definitions");
                var actions = plan.GetOrCreateFolder(featureFolder, $"include_{featureName}_actions", "actions");
                var internals = plan.GetOrCreateFolder(featureFolder, $"include_{featureName}_internals", "internals");

                // Index .h files in each sub-folder (writable — AI can create here)
                plan.IndexFiles(definitions, "h", $"include_{featureName}_definitions_h_");
                plan.IndexFiles(actions, "h", $"include_{featureName}_actions_h_");
                plan.IndexFiles(internals, "h", $"include_{featureName}_internals_h_");

                // Index any stray .h files directly in the feature folder (read-only — outside structured sub-folders)
                plan.IndexFiles(featureFolder, "h", $"include_{featureName}_h_", true);
            }

            // ===== src/ =====
            var src = plan.GetOrCreateFolder("src", "src");

            // Index .c and .cpp files directly in src/ root (writable)
            plan.IndexFiles(src, "c", "c_");
            plan.IndexFiles(src, "cpp", "cpp_");

            // Index any stray .h files in src/ root (read-only — not part of new structure)
            plan.IndexFiles(src, "h", "h_", true);

            // Index feature folders in src/
            var srcDirs = Directory.GetDirectories(Path.Combine(plan.RootDirectory, src.RelativePath));
            foreach (var dir in srcDirs)
            {
                var dirInfo = new DirectoryInfo(dir);
                var featureName = dirInfo.Name.ToLowerInvariant();
                var featureFolder = plan.GetOrCreateFolder(src, $"src_{featureName}", dirInfo.Name);

                // Ensure structured sub-folders exist: actions, internals (no definitions in src/)
                var actions = plan.GetOrCreateFolder(featureFolder, $"src_{featureName}_actions", "actions");
                var internals = plan.GetOrCreateFolder(featureFolder, $"src_{featureName}_internals", "internals");

                // Index .c and .cpp files in each sub-folder (writable — AI can create here)
                plan.IndexFiles(actions, "c", $"src_{featureName}_actions_c_");
                plan.IndexFiles(actions, "cpp", $"src_{featureName}_actions_cpp_");
                plan.IndexFiles(internals, "c", $"src_{featureName}_internals_c_");
                plan.IndexFiles(internals, "cpp", $"src_{featureName}_internals_cpp_");

                // Index any stray .h files directly in the feature folder (read-only — outside structured sub-folders)
                plan.IndexFiles(featureFolder, "h", $"src_{featureName}_h_", true);
            }

            // ===== lib/ (read-only) =====
            var lib = plan.GetOrCreateFolder("lib", "lib");
            var libDirs = Directory.GetDirectories(Path.Combine(plan.RootDirectory, lib.RelativePath));
            foreach (var dir in libDirs)
            {
                var dirInfo = new DirectoryInfo(dir);
                var folder = plan.GetOrCreateFolder(lib, dirInfo.Name, dirInfo.Name);
                plan.IndexFiles(folder, "json", $"{folder.Name}_json_", true);
                plan.IndexFiles(folder, "properties", $"{folder.Name}_props_", true);
                plan.IndexFiles(folder, "h", $"{folder.Name}_h_", true);
                var libSrc = Path.Combine(dir, "src");
                if (Directory.Exists(libSrc))
                {
                    var libSrcFolder = plan.GetOrCreateFolder(folder, $"{folder.Name}_src_", "src");
                    plan.IndexFiles(libSrcFolder, "cpp", $"{folder.Name}_src_cpp_", true);
                    plan.IndexFiles(libSrcFolder, "c", $"{folder.Name}_src_c_", true);
                    plan.IndexFiles(libSrcFolder, "h", $"{folder.Name}_src_h_", true);
                }
            }

            // ===== data/ (writable) =====
            var data = plan.GetOrCreateFolder("data", "data");
            plan.IndexFiles(data, "txt", "data_txt_");
            plan.IndexFiles(data, "json", "data_json_");
        }

        public static void IndexPlatformIOEspIdf(this DevelopPlan plan)
        {
            // Root-level ESP-IDF files (read-only)
            plan.IndexFiles("txt", "txt_", true);
            plan.IndexFiles("csv", "csv_", true);

            // src/ txt files (ESP-IDF specific, read-only)
            var src = plan.GetOrCreateFolder("src", "src");
            plan.IndexFiles(src, "txt", "src_txt_", true);

            // components/ (read-only)
            var components = plan.GetOrCreateFolder("components", "components");
            var compDirs = Directory.GetDirectories(Path.Combine(plan.RootDirectory, components.RelativePath));
            foreach (var item in compDirs)
            {
                var info = new DirectoryInfo(item);
                var folder = plan.GetOrCreateFolder(components, $"components_{info.Name}", info.Name);
                plan.IndexFiles(folder, "yml", $"components_{folder.Name}_yml_", true);
            }

            // managed_components/ (read-only)
            var mComponents = plan.GetOrCreateFolder("managed_components", "managed_components");
            var mDirs = Directory.GetDirectories(Path.Combine(plan.RootDirectory, mComponents.RelativePath));
            foreach (var item in mDirs)
            {
                var info = new DirectoryInfo(item);
                var folder = plan.GetOrCreateFolder(mComponents, $"managed_components_{info.Name}", info.Name);
                plan.IndexFiles(folder, "yml", $"managed_components_{folder.Name}_yml_", true);
            }
        }
    }
}
