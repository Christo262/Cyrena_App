namespace Cyrena.Screens.Models;

/// <summary>
/// Addressable handle for a single browser-side <c>MediaStream</c>.
/// Strings, not <c>MediaStream</c> references — JS holds the only
/// real references; .NET uses this token to ask JS to capture from
/// or release that specific stream.
/// </summary>
public readonly record struct ScreenToken(string Value)
{
    public static ScreenToken Empty { get; } = new(string.Empty);

    public bool IsEmpty => string.IsNullOrEmpty(Value);

    public override string ToString() => Value;
}
