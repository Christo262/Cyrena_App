using Cyrena.Options;

namespace Cyrena.Contracts
{
    public interface IWindowLauncher : IDisposable
    {
        void Show(string url, int width, int height, string title = "Cyréna");
    }
}
