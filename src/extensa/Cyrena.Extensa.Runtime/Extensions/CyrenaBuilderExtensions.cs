using Cyrena.Extensa.Contracts;
using Cyrena.Extensa.Loader.Contracts;
using Cyrena.Extensa.Loader.Models;
using Cyrena.Extensa.Loader.Services;
using Cyrena.Extensa.Models;
using Cyrena.Extensa.Options;
using Cyrena.Options;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Reflection;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddExtensa(this CyrenaBuilder builder, Action<ExtensaOptions> options)
        {
            var o = new ExtensaOptions();
            options(o);
            if (string.IsNullOrEmpty(o.ExtensionInfoFileName) || string.IsNullOrEmpty(o.ExtensionsDirectory) || string.IsNullOrEmpty(o.InstallationsDirectory))
                throw new InvalidOperationException("Invalid Extensa configuration.");

            builder.AddFeatureOption<ExtensaOptions>(o);
            builder.RunStartupUninstaller();
            builder.RunStartupInstaller();
            var rgstry = new ExtensionRegistry();
            builder.AddFeatureOption<IExtensionRegistry>(rgstry);

            try
            {
                var readme_i = Path.Combine(o.InstallationsDirectory, "README.txt");
                if (!File.Exists(readme_i))
                {
                    var readme = Resources.Read(typeof(ExtensionRegistry).Assembly, "Cyrena.Extensa.Resources.install-readme.md");
                    File.WriteAllText(readme_i, readme);
                }
            }
            catch { }

            builder.BuildActions.Add(framework =>
            {
                ExtensaOptions options = framework.GetFeatureOption<ExtensaOptions>();
                builder.Services.Configure<ExtensaOptions>(o =>
                {
                    o.ExtensionInfoFileName = options.ExtensionInfoFileName;
                    o.ExtensionsDirectory = options.ExtensionsDirectory;
                    o.InstallationsDirectory = options.InstallationsDirectory;
                });
                framework.LoadManifestInfo();
                var registry = builder.GetFeatureOption<IExtensionRegistry>();
                framework.LoadExtensions();
                builder.Services.AddSingleton<IExtensionRegistry>(registry);
            });
            return builder;
        }

        public static CyrenaBuilder AddExtension<TExtension>(this CyrenaBuilder builder, string id, string name, Version version, string? description = null)
            where TExtension : class, IExtension, new()
        {
            var ext = new TExtension();
            ext.BuildExtension(builder);

            var registry = builder.GetFeatureOption<IExtensionRegistry>();
            registry.AddExtension(new Extensa.Loader.Models.LoadedExtension()
            {
                Id = id,
                Name = name,
                Version = version,
                Description = description,
                Status = Extensa.Loader.Models.ExtensionStatus.Runtime
            });
            return builder;
        }

        private static void RunStartupUninstaller(this CyrenaBuilder builder)
        {
            var options = builder.GetFeatureOption<ExtensaOptions>();
            if (!Directory.Exists(options.InstallationsDirectory))
                return;
            var script = Path.Combine(options.InstallationsDirectory, "uninstall.json");
            if(!File.Exists(script))
                return;
            try
            {
                var json = File.ReadAllText(script);
                string[]? ids = JsonConvert.DeserializeObject<string[]>(json);
                if (ids == null)
                    throw new Exception();
                foreach(var item in ids)
                {
                    var path = Path.Combine(options.ExtensionsDirectory, item);
                    if(Directory.Exists(path))
                        Directory.Delete(path, true);
                }
            }
            catch
            {

            }
            finally
            {
                File.Delete(script);
            }
        }

        private static void RunStartupInstaller(this CyrenaBuilder builder)
        {
            var options = builder.GetFeatureOption<ExtensaOptions>();
            if (!Directory.Exists(options.InstallationsDirectory))
                Directory.CreateDirectory(options.InstallationsDirectory);
            if (!Directory.Exists(options.ExtensionsDirectory))
                Directory.CreateDirectory(options.ExtensionsDirectory);
            var files = Directory.GetFiles(options.InstallationsDirectory, "*.zip");
            foreach (var file in files)
            {
                var info = new FileInfo(file);
                var destination = Path.Combine(options.ExtensionsDirectory, info.Name.Replace(".zip", ""));
                if (Directory.Exists(destination))
                    Directory.Delete(destination, true);
                ZipFile.ExtractToDirectory(file, destination);
                File.Delete(file);
            }
        }

        private static void LoadManifestInfo(this CyrenaBuilder builder)
        {
            var registry = builder.GetFeatureOption<IExtensionRegistry>();
            var options = builder.GetFeatureOption<ExtensaOptions>();
            if (!Directory.Exists(options.ExtensionsDirectory))
                Directory.CreateDirectory(options.ExtensionsDirectory);
            var dirs = Directory.GetDirectories(options.ExtensionsDirectory);
            foreach (var dir in dirs)
            {
                var path = Path.Combine(dir, options.ExtensionInfoFileName);
                if (File.Exists(path))
                {
                    var loadedExtension = new LoadedExtension()
                    {
                        Path = dir,
                    };
                    try
                    {
                        var json = File.ReadAllText(path);
                        var extensionInfo = JsonConvert.DeserializeObject<ExtensionInfo>(json);
                        if (extensionInfo == null)
                            throw new NullReferenceException($"Unable to deserialize extension info from {path}");
                        loadedExtension.Description = extensionInfo.Description;
                        loadedExtension.Id = extensionInfo.Id;
                        loadedExtension.Version = extensionInfo.Version;
                        loadedExtension.Name = extensionInfo.Name;
                        loadedExtension.Status = ExtensionStatus.Unloaded;
                        loadedExtension.EntryAssembly = extensionInfo.EntryAssemblyFile;
                        loadedExtension.Dependencies = extensionInfo.Dependencies;
                    }
                    catch (Exception ex)
                    {
                        loadedExtension.Errors.Add(ex);
                        loadedExtension.Status = ExtensionStatus.Unloaded;
                    }
                    registry.AddExtension(loadedExtension);
                }
            }
        }

        private static void LoadExtensions(this CyrenaBuilder builder)
        {
            var registry = builder.GetFeatureOption<IExtensionRegistry>();
            var extensions = new List<LoadedExtension>();
            extensions.AddRange(registry.Extensions);
            var ass = AppDomain.CurrentDomain.GetAssemblies().ToList();
            while (extensions.Count > 0)
            {
                var extension = extensions.First();
                builder.LoadExtension(extensions, extension, ass);
            }
        }

        private static bool LoadExtension(this CyrenaBuilder builder, List<LoadedExtension> extensions, LoadedExtension target, List<Assembly> assemblies)
        {
            var registry = builder.GetFeatureOption<IExtensionRegistry>();
            try
            {
                var deps = target.Dependencies;
                foreach (var d in deps)
                {
                    var dext = extensions.FirstOrDefault(x => x.Id == d.Id) ?? registry.Extensions.FirstOrDefault(x => x.Id == d.Id);
                    if (dext == null || dext.Errors.Any())
                        throw new UnmetDependencyException(target.Id, target.Version, d.Id, d.MinVersion);
                    if (dext.Version < d.MinVersion)
                        throw new DependencyVersionException(target.Id, target.Version, d.Id, d.MinVersion);
                    if (dext.Dependencies.Any(x => x.Id == target.Id))
                        throw new CircularDependencyException(target.Id, target.Version, dext.Id, dext.Version);
                    if (dext.Status == ExtensionStatus.Unloaded)
                    {
                        var e = builder.LoadExtension(extensions, dext, assemblies);
                        if (!e)
                            throw new UnmetDependencyException(target.Id, target.Version, d.Id, d.MinVersion);
                    }
                }

                if (target.Status == ExtensionStatus.Unloaded)
                {
                    if (target.EntryAssembly == null)
                        throw new Exception("Unable to construct entry point. (E_NULL)");

                    var sharedAssembly = AppDomain.CurrentDomain.GetAssemblies()
                            .FirstOrDefault(a => a.GetName().Name == "Cyrena.Extensa.Core");
                    var iExtensionType = sharedAssembly.GetType("Cyrena.Extensa.Contracts.IExtension");

                    var context = new ExtensionLoadContext(target.Path, assemblies);
                    var assembly = context.LoadFromAssemblyPath(Path.Combine(target.Path, target.EntryAssembly));
                    var types = assembly.GetTypes();
                    var iExtension = types.FirstOrDefault(x => iExtensionType.IsAssignableFrom(x));
                    if (iExtension == null)
                        throw new Exception($"Extension does not have a valid entry point");
                    IExtension? main = Activator.CreateInstance(iExtension) as IExtension;
                    if (main == null)
                        throw new InvalidOperationException("Unable to construct entry point");
                    if (target.RequireFrameworkBuilder)
                    {
                        main.BuildExtension(builder);
                        target.Status = ExtensionStatus.Loaded;
                    }
                    else
                        target.Status = ExtensionStatus.Runtime;
                }

                var re = registry.Extensions.First(x => x.Id == target.Id);
                re.Status = target.Status;
                re.Errors = target.Errors;
                extensions.Remove(target);
                return true;
            }
            catch (Exception ex)
            {
                target.Errors.Add(ex);
                target.Status = ExtensionStatus.Unloaded;

                var re = registry.Extensions.First(x => x.Id == target.Id);
                re.Status = target.Status;
                re.Errors = target.Errors;
                extensions.Remove(target);
                return false;
            }
        }
    }
}
