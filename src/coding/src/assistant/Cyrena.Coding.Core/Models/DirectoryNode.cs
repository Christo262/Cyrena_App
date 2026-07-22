namespace Cyrena.Coding.Models;

public class DirectoryNode
{
    public string Id { get; set; } = null!;
    public string Name { get; set; }= null!;
    public string Path { get; set; }= null!;
    public List<DirectoryNode> Children { get; set; } = [];
}