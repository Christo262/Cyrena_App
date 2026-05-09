using Cyrena.Synthesis.Models;

namespace Cyrena.Synthesis.Contracts
{
    public interface ICapabilityResultWriter
    {
        void Text(string key, string value);
        void Number(string key, double value);
        void Boolean(string key, bool value);
        void Json<T>(string key, T value);
        void Error(string code, string message);
        CapabilityResultBag ResultBag { get; }
    }
}
