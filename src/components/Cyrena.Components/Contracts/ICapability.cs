namespace Cyrena.Contracts
{
    public interface ICapability
    {
        Type Component { get; }
    }

    internal class Capability : ICapability
    {
        public Capability(Type component)
        {
            Component = component;
        }

        public Type Component { get; }
    }
}
