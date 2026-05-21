namespace Cyrena.Models
{
    public interface IWindowHandle : IDisposable
    {
        event EventHandler<EventArgs>? Closing;
        bool Disposed { get; }
        void Close();
    }
}
