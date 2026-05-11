namespace Cyrena.Components.Shared
{
    internal class HeadOutletStateChangeTracker
    {
        public event EventHandler<EventArgs>? OnChanged;

        public void Invoke()
        {
            OnChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
