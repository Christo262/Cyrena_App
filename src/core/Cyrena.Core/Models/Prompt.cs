namespace Cyrena.Models
{
    public sealed class Prompt
    {
        public Prompt()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; init; }
        public int Order { get; init; }
        public string Content { get; init; } = default!;
    }
}
