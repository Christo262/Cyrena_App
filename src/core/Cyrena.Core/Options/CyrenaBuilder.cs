using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Cyrena.Options
{
    public sealed class CyrenaBuilder
    {
        public static readonly string AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".cyrena");
        public static readonly string UserContentDirectory = Path.Combine(AppDataDirectory, "public");

        public CyrenaBuilder(IServiceCollection services)
        {
            Services = services;
            FeatureAssemblies = new Dictionary<string, IList<Assembly>>();
            FeatureOptions = new Dictionary<string, object>();
            BuildActions = new List<Action<CyrenaBuilder>>();
            RunActions = new List<Action<IServiceProvider, CancellationToken>>();
        }

        public IServiceCollection Services { get; }
        public IDictionary<string, IList<Assembly>> FeatureAssemblies { get; }
        public IDictionary<string, object> FeatureOptions { get; }
        public IList<Action<CyrenaBuilder>> BuildActions { get; }
        public IList<Action<IServiceProvider, CancellationToken>> RunActions { get; }

        public void AddBuildAction(Action<CyrenaBuilder> action)
        {
            BuildActions.Add(action);
        }

        public void AddRunAction(Action<IServiceProvider, CancellationToken> action)
        {
            RunActions.Add(action);
        }

        public void Build()
        {
            for (int i = 0; i < BuildActions.Count; i++)
                BuildActions[i].Invoke(this);
            Services.AddSingleton(new CyrenaOptions(FeatureAssemblies));
        }

        private bool _disposed { get; set; }
    }

    public sealed class CyrenaOptions
    {
        public CyrenaOptions(IDictionary<string, IList<Assembly>> featureAssemblies)
        {
            FeatureAssemblies = featureAssemblies;
        }

        public IDictionary<string, IList<Assembly>> FeatureAssemblies { get; }
    }
}
