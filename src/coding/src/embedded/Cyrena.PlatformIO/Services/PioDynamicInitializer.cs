using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.PlatformIO.Contracts;

namespace Cyrena.PlatformIO.Services;

internal class PioDynamicInitializer(IDevelopPlanService planService, IEnvironmentController envs) : IDynamicPlanInitializer
{
    public void Initialize()
    {
        ScanPioFiles(planService.Plan);
    }

    public void RunIndex()
    {
        ScanPioFiles(planService.Plan);
    }

    private void ScanPioFiles(DevelopPlan plan)
    {
        plan.Discover("ini", true, true);
            var src = plan.GetOrCreateFolder("src", "src");
            plan.Discover(src, "c",false);
            plan.Discover(src, "cpp",false);
            plan.Discover(src, "h",false);
            plan.Discover(src, "hpp",false);
            
            var include =  plan.GetOrCreateFolder("include", "include");
            plan.Discover(include, "c",false);
            plan.Discover(include, "cpp",false);
            plan.Discover(include, "h",false);
            plan.Discover(include, "hpp",false);
            
            var data = plan.GetOrCreateFolder("data", "data");
            plan.Discover(data, "txt",false);
            plan.Discover(data, "json",false);

            var environments = envs.Environments;
            string[] readFolders = ["lib"];
            if (environments.Any(env => env.Framework?.Split(',', StringSplitOptions.TrimEntries)
                    .Any(f => f.Equals("espidf", StringComparison.OrdinalIgnoreCase)) == true))
            {
                readFolders = ["lib", "components", "managed_components"];
            }

            foreach (var rd in readFolders)
            {
                var lib = plan.GetOrCreateFolder(rd, rd);
                if (Directory.Exists(Path.Combine(plan.RootDirectory, lib.RelativePath)))
                {
                    var libs = Directory.GetDirectories(Path.Combine(plan.RootDirectory, lib.RelativePath));
                    foreach (var item in libs)
                    {
                        var inf = new DirectoryInfo(item);
                        var lbFolder = plan.GetOrCreateFolder(lib, $"{rd}_{inf.Name.ToLower()}", inf.Name);
                        plan.IndexFiles(lbFolder,"h", $"{lbFolder.Id}_",true);
                        if (Directory.Exists(Path.Combine(plan.RootDirectory, lbFolder.RelativePath, "src")))
                        {
                            var lbSrc = plan.GetOrCreateFolder(lbFolder, $"{lbFolder.Id}_src", "src");
                            plan.IndexFiles(lbSrc,"h", $"{lbSrc.Id}_",true);
                        }
                        if (Directory.Exists(Path.Combine(plan.RootDirectory, lbFolder.RelativePath, "include")))
                        {
                            var lbInclude = plan.GetOrCreateFolder(lbFolder, $"{lbFolder.Id}_include", "include");
                            plan.IndexFiles(lbInclude,"h", $"{lbInclude.Id}_",true);
                        }
                    }
                }
            }
    }
}