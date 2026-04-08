namespace Cyrena.Contracts
{
    public interface IShortcut
    {
        string Title { get; }
        string Description { get; }
        string Icon { get; }
        string Color { get; }
        string Category { get; }
        string[] Tags { get; }

        Task OnClick();
    }
}
