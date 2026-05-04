namespace MyConsoleApp.Models;

/// <summary>
/// Options parsed from the "greet" command.
/// </summary>
public class GreetOptions
{
    /// <summary>
    /// The name to greet. Required positional argument.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional greeting style (e.g., "formal", "casual").
    /// </summary>
    public string Style { get; set; } = "casual";

    /// <summary>
    /// Number of times to repeat the greeting.
    /// </summary>
    public int Count { get; set; } = 1;

    /// <summary>
    /// Whether to shout (uppercase) the greeting.
    /// </summary>
    public bool Shout { get; set; } = false;
}
