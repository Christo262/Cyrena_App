namespace MyConsoleApp.Models;

/// <summary>
/// Options parsed from the "calculate" command.
/// </summary>
public class CalculateOptions
{
    /// <summary>
    /// The left-hand operand.
    /// </summary>
    public double Left { get; set; }

    /// <summary>
    /// The right-hand operand.
    /// </summary>
    public double Right { get; set; }

    /// <summary>
    /// The operation to perform: add, subtract, multiply, divide.
    /// </summary>
    public string Operation { get; set; } = "add";

    /// <summary>
    /// Number of decimal places to round the result to.
    /// </summary>
    public int Precision { get; set; } = 2;
}
