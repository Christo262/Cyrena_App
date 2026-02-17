namespace Cyrena.Contracts
{
    public interface IShortcut
    {
        string Title { get; }
        string Description { get; }
        string Icon { get; }
        string Color { get; }

        Task OnClick();
    }
}
