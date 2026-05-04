using Cyrena.PlatformIO.Contracts;
using Cyrena.PlatformIO.Models;

namespace Cyrena.PlatformIO.Services
{
    internal class EnvironmentController : IEnvironmentController
    {
        private readonly List<PlatformIOEnvironment> _envs;
        private PlatformIOEnvironment? _current { get; set; }
        public EnvironmentController(List<PlatformIOEnvironment> envs)
        {
            _envs = envs;
            _current = _envs.FirstOrDefault();
        }

        public PlatformIOEnvironment? Current => _current;
        public IReadOnlyList<PlatformIOEnvironment> Environments => _envs;

        public void SetCurrentEnvironment(string name)
        {
            _current = _envs.FirstOrDefault(x => x.Name == name);
        }
    }
}
