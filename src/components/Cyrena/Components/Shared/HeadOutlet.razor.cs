namespace Cyrena.Components.Shared
{
    public class HeadOutletStateChangeTracker
    {
        public event EventHandler<EventArgs>? OnChanged;

        public void Invoke()
        {
            OnChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
