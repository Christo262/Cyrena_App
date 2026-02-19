using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Cyrena.Options
{
    public sealed class CyrenaBuilder
    {
        public CyrenaBuilder(IServiceCollection services)
        {
            Services = services;
            FeatureAssemblies = new Dictionary<string, IList<Assembly>>();
            FeatureOptions = new Dictionary<string, object>();
            BuildActions = new List<Action<CyrenaBuilder>>();
        }

        public IServiceCollection Services { get; }
        public IDictionary<string, IList<Assembly>> FeatureAssemblies { get; }
        public IDictionary<string, object> FeatureOptions { get; }
        public IList<Action<CyrenaBuilder>> BuildActions { get; }

        public void AddBuildAction(Action<CyrenaBuilder> action)
        {
            BuildActions.Add(action);
        }

        public void Build()
        {
            for (int i = 0; i < BuildActions.Count; i++)
                BuildActions[i].Invoke(this);
            Services.AddSingleton(new CyrenaOptions(FeatureAssemblies));
        }
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
