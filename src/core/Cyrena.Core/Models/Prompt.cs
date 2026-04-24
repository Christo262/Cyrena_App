namespace Cyrena.Models
{
    public sealed class Prompt : Entity
    {
        public Prompt()
        {
            Id = Guid.NewGuid().ToString();
        }

        public int Order { get; init; }
        public string Content { get; init; } = default!;
    }
}
