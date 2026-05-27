using Cyrena.Options;

namespace Cyrena.Contracts
{
    public interface IWindowLauncher : IDisposable
    {
        void ShowMain(ApplicationOptions options);
        void Show(string url, int width, int height);
    }
}
