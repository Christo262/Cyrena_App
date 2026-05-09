using System.Reflection;
using System.Runtime.Loader;

namespace Cyrena.Synthesis.Models
{
    /// <summary>
    /// Custom AssemblyLoadContext for dynamically compiled F# dynamic capabilities.
    ///
    /// IMPORTANT: AssemblyLoadContext is NOT a security boundary.
    /// It is used for assembly isolation and collectible assemblies only.
    /// The true security boundary is the planned worker process isolation.
    /// </summary>
    internal class ScriptAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly Dictionary<string, Assembly> _loadedAssemblies = new();

        public ScriptAssemblyLoadContext() : base(isCollectible: true)
        {
            Resolving += ScriptAssemblyLoadContext_Resolving;
        }

        private Assembly? ScriptAssemblyLoadContext_Resolving(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            // 1. Check if already loaded in default ALC
            var defaultAssembly = AssemblyLoadContext.Default.Assemblies
                .FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
            if (defaultAssembly != null)
                return defaultAssembly;

            // 2. Check if already loaded in this ALC
            if (_loadedAssemblies.TryGetValue(assemblyName.Name ?? string.Empty, out var assembly))
                return assembly;

            // 3. Try to load from main app's directory
            var frameworkDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
            if (!string.IsNullOrEmpty(frameworkDir) && ! string.IsNullOrEmpty(assemblyName.Name))
            {
                var refPath = Path.Combine(frameworkDir, $"{assemblyName.Name}.dll");
                if(File.Exists(refPath))
                {
                    var mainAppAssembly = LoadFromAssemblyPath(refPath);
                    _loadedAssemblies.Add(assemblyName.Name, mainAppAssembly);
                    return mainAppAssembly;
                }
            }
            return null;
        }

        public void PreloadAssembly(string name, Assembly assembly)
        {
            _loadedAssemblies[name] = assembly;
        }

        public void PreloadAssembly(string path)
        {
            if (File.Exists(path))
            {
                var mainAppAssembly = LoadFromAssemblyPath(path);
                var name = mainAppAssembly.GetName().Name;
                if(!string.IsNullOrEmpty(name) && !_loadedAssemblies.ContainsKey(name))
                    _loadedAssemblies.Add(mainAppAssembly.GetName().Name!, mainAppAssembly);
            }
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            if (_loadedAssemblies.TryGetValue(assemblyName.Name ?? string.Empty, out var assembly))
            {
                return assembly;
            }

            // Fallback: try to load from default context (needed for FSharp.Core and other referenced assemblies)
            try
            {
                return Default.LoadFromAssemblyName(assemblyName);
            }
            catch
            {
                return null;
            }
        }

        

        public Assembly LoadFromBytes(byte[] assemblyBytes)
        {
            return LoadFromStream(new MemoryStream(assemblyBytes));
        }
    }
}
