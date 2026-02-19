namespace Cyrena.Contracts
{
    public interface IToolbarComponent
    {
        Type Component { get; }
        ToolbarAlignment Alignment { get; }
    }

    public enum ToolbarAlignment
    {
        Start, End
    }

    internal class ToolbarComponent : IToolbarComponent
    {
        public ToolbarComponent(Type component, ToolbarAlignment alignment)
        {
            Component = component;
            Alignment = alignment;
        }

        public Type Component { get; }
        public ToolbarAlignment Alignment { get; }
    }
}
