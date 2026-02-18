using Cyrena.Developer.Models;
using Cyrena.Developer.Extensions;
using Cyrena.Extensions;
using Cyrena.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.PlatformIO.Extensions
{
    public static class ProjectPlanExtensions
    {
        public static void IndexPlatformIODefaultPlan(this DevelopPlan plan)
        {
            var src = plan.GetOrCreateFolder("src", "src");
            plan.IndexFiles(src, "c", "c_");
            plan.IndexFiles(src, "cpp", "cpp_");
            plan.IndexFiles(src, "h", "h_");
            var srcDirs = Directory.GetDirectories(Path.Combine(plan.RootDirectory, src.RelativePath));
            foreach (var dir in srcDirs)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dir);
                var folder = plan.GetOrCreateFolder(src, dirInfo.Name, dirInfo.Name);
                plan.IndexFiles(folder, "h", $"{folder.Name}_h_");
                plan.IndexFiles(folder, "cpp", $"{folder.Name}_cpp_");
                plan.IndexFiles(folder, "c", $"{folder.Name}_c_");
            }


            var include = plan.GetOrCreateFolder("include", "include");
            plan.IndexFiles(include, "h", "include_h_");

            var lib = plan.GetOrCreateFolder("lib", "lib");
            var libDirs = Directory.GetDirectories(Path.Combine(plan.RootDirectory, lib.RelativePath));
            foreach (var dir in libDirs)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dir);
                var folder = plan.GetOrCreateFolder(lib, dirInfo.Name, dirInfo.Name);
                plan.IndexFiles(folder, "json", $"{folder.Name}_json_", true); //index library.json as readonly
                plan.IndexFiles(folder, "properties", $"{folder.Name}_props_", true); //index library.properties as read only
                plan.IndexFiles(folder, "h", $"{folder.Name}_h_", true); //index headers as read only
                var libSrc = Path.Combine(dir, "src");
                if (Directory.Exists(libSrc))
                {
                    var libSrcFolder = plan.GetOrCreateFolder(folder, $"{folder.Name}_src_", "src");
                    plan.IndexFiles(libSrcFolder, "cpp", $"{folder.Name}_src_cpp_", true);
                    plan.IndexFiles(libSrcFolder, "c", $"{folder.Name}_src_c_", true);
                    plan.IndexFiles(libSrcFolder, "h", $"{folder.Name}_src_h_", true);
                }
            }

            var data = plan.GetOrCreateFolder("data", "data");
            plan.IndexFiles(data, "txt", "data_txt_"); //some basic data stuff
            plan.IndexFiles(data, "json", "data_json_"); //some basic data stuff
        }

        public static void IndexPlatformIOEspIdf(this DevelopPlan plan)
        {
            plan.IndexFiles("txt", "txt_", true);
            plan.IndexFiles("csv", "csv_"); //partitions.csv

            var src = plan.GetOrCreateFolder("src", "src");
            plan.IndexFiles("txt", "src_txt_", true);

            var components = plan.GetOrCreateFolder("components", "components");
            var dirs = Directory.GetDirectories(Path.Combine(plan.RootDirectory, components.RelativePath));
            foreach(var item in dirs)
            {
                var info = new DirectoryInfo(item);
                var folder = plan.GetOrCreateFolder(components, $"components_{info.Name}", info.Name);
                plan.IndexFiles(folder, "yml", $"components_{folder.Name}_yml_", true);
            }

            var m_components = plan.GetOrCreateFolder("managed_components", "managed_components");
            var m_dirs = Directory.GetDirectories(Path.Combine(plan.RootDirectory, m_components.RelativePath));
            foreach (var item in m_dirs)
            {
                var info = new DirectoryInfo(item);
                var folder = plan.GetOrCreateFolder(m_components, $"managed_components_{info.Name}", info.Name);
                plan.IndexFiles(folder, "yml", $"managed_components_{folder.Name}_yml_", true);
            }
        }
    }
}
