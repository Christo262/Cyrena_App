namespace Cyrena.Models
{
    public interface IEntity
    {
        string Id { get; set; }
    }

    public abstract class Entity : IEntity
    {
        public virtual string Id { get; set; } = default!;
    }
}
