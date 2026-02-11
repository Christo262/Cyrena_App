using Cyrena.PlatformIO.Models;

namespace Cyrena.PlatformIO.Contracts
{
    public interface IEnvironmentController
    {
        PlatformIOEnvironment? Current { get; }
        IReadOnlyList<PlatformIOEnvironment> Environments { get; }
        void SetCurrentEnvironment(string name);
    }
}
