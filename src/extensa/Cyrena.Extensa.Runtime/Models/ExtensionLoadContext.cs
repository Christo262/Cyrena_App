using System.Reflection;
using System.Runtime.Loader;

namespace Cyrena.Extensa.Loader.Models
{
    internal class ExtensionLoadContext : AssemblyLoadContext
    {
        private readonly string _extensionPath;
        private readonly List<Assembly> _assemblies;
        public ExtensionLoadContext(string extensionPath, List<Assembly> assemblies)
        {
            _extensionPath = extensionPath;
            _assemblies = assemblies;
            Resolving += OnResolving;
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var ext = _assemblies.FirstOrDefault(x => x.GetName().Name == assemblyName.Name);
            if(ext != null) 
                return ext;
            string assemblyPath = Path.Combine(_extensionPath, $"{assemblyName.Name}.dll");
            if (File.Exists(assemblyPath))
            {
                var msit = LoadFromAssemblyPath(assemblyPath);
                _assemblies.Add(msit);
                return msit;
            }

            return null;
        }

        private Assembly? OnResolving(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            var ext = _assemblies.FirstOrDefault(x => x.GetName().Name == assemblyName.Name);
            if (ext != null)
                return ext;
            string dependencyPath = Path.Combine(_extensionPath, $"{assemblyName.Name}.dll");
            if (File.Exists(dependencyPath))
            {
                var msit = LoadFromAssemblyPath(dependencyPath);
                _assemblies.Add(msit);
                return msit;
            }

            return null;
        }
    }
}
