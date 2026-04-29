using System.Globalization;
using System.Text;
using MyConsoleApp.Contracts;
using MyConsoleApp.Models;

namespace MyConsoleApp.Services;

/// <summary>
/// Routes CLI arguments to the correct command handler and parses parameters.
/// Demonstrates how CLI apps distinguish commands and parse options.
/// </summary>
public class CliCommandService : ICliCommandService
{
    // ═══════════════════════════════════════════════════════════════
    //  COMMAND ROUTING
    //  The first argument after the app name is the "command".
    //  We switch on it to decide which handler to run.
    // ═══════════════════════════════════════════════════════════════
    public int Execute(string[] args)
    {
        if (args.Length == 0 || args[0] is "--help" or "-h" or "help")
        {
            PrintHelp();
            return 0;
        }

        string command = args[0].ToLowerInvariant();
        string[] remainingArgs = args[1..]; // everything after the command

        return command switch
        {
            "greet" => HandleGreet(remainingArgs),
            "calculate" or "calc" => HandleCalculate(remainingArgs),
            "echo" => HandleEcho(remainingArgs),
            _ => HandleUnknownCommand(command)
        };
    }

    // ═══════════════════════════════════════════════════════════════
    //  GREET COMMAND
    //  Usage: greet <name> [--style formal|casual] [--count N] [--shout]
    // ═══════════════════════════════════════════════════════════════
    private static int HandleGreet(string[] args)
    {
        var options = new GreetOptions();
        int i = 0;

        // ── Positional argument: name ─────────────────────────────
        if (i < args.Length && !args[i].StartsWith('-'))
        {
            options.Name = args[i];
            i++;
        }
        else
        {
            Console.WriteLine("Error: greet requires a <name> argument.");
            Console.WriteLine("Usage: greet <name> [--style formal|casual] [--count N] [--shout]");
            return 1;
        }

        // ── Named options ─────────────────────────────────────────
        while (i < args.Length)
        {
            string arg = args[i];

            if (arg is "--style" or "-s")
            {
                if (i + 1 >= args.Length) { Console.WriteLine("Error: --style requires a value."); return 1; }
                options.Style = args[++i];
            }
            else if (arg is "--count" or "-c")
            {
                if (i + 1 >= args.Length || !int.TryParse(args[++i], out int count))
                { Console.WriteLine("Error: --count requires an integer value."); return 1; }
                options.Count = count;
            }
            else if (arg is "--shout")
            {
                options.Shout = true; // boolean flag: no value needed
            }
            else if (arg is "--help" or "-h")
            {
                Console.WriteLine("Usage: greet <name> [--style formal|casual] [--count N] [--shout]");
                return 0;
            }
            else
            {
                Console.WriteLine($"Error: Unknown option '{arg}' for greet command.");
                return 1;
            }
            i++;
        }

        // ── Execute ─────────────────────────────────────────────────
        string greeting = options.Style.ToLowerInvariant() switch
        {
            "formal" => $"Good day, {options.Name}.",
            "casual" => $"Hey {options.Name}!",
            _ => $"Hello, {options.Name}!"
        };

        if (options.Shout) greeting = greeting.ToUpperInvariant();

        for (int r = 0; r < options.Count; r++)
            Console.WriteLine(greeting);

        return 0;
    }

    // ═══════════════════════════════════════════════════════════════
    //  CALCULATE COMMAND
    //  Usage: calculate <left> <operation> <right> [--precision N]
    //  Or:    calculate --left N --op add --right N [--precision N]
    // ═══════════════════════════════════════════════════════════════
    private static int HandleCalculate(string[] args)
    {
        var options = new CalculateOptions();
        int i = 0;

        // Try positional parsing first: <left> <op> <right>
        if (args.Length >= 3 && !args[0].StartsWith('-') && !args[1].StartsWith('-') && !args[2].StartsWith('-'))
        {
            if (!double.TryParse(args[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double left))
            { Console.WriteLine($"Error: '{args[0]}' is not a valid number."); return 1; }
            if (!double.TryParse(args[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double right))
            { Console.WriteLine($"Error: '{args[2]}' is not a valid number."); return 1; }

            options.Left = left;
            options.Operation = args[1].ToLowerInvariant();
            options.Right = right;
            i = 3;
        }
        else
        {
            // Named argument parsing
            while (i < args.Length)
            {
                string arg = args[i];
                if (arg is "--left" or "-l")
                {
                    if (i + 1 >= args.Length || !double.TryParse(args[++i], NumberStyles.Any, CultureInfo.InvariantCulture, out double left))
                    { Console.WriteLine("Error: --left requires a numeric value."); return 1; }
                    options.Left = left;
                }
                else if (arg is "--right" or "-r")
                {
                    if (i + 1 >= args.Length || !double.TryParse(args[++i], NumberStyles.Any, CultureInfo.InvariantCulture, out double right))
                    { Console.WriteLine("Error: --right requires a numeric value."); return 1; }
                    options.Right = right;
                }
                else if (arg is "--operation" or "--op" or "-o")
                {
                    if (i + 1 >= args.Length) { Console.WriteLine("Error: --operation requires a value."); return 1; }
                    options.Operation = args[++i].ToLowerInvariant();
                }
                else if (arg is "--precision" or "-p")
                {
                    if (i + 1 >= args.Length || !int.TryParse(args[++i], out int precision))
                    { Console.WriteLine("Error: --precision requires an integer."); return 1; }
                    options.Precision = precision;
                }
                else if (arg is "--help" or "-h")
                {
                    PrintCalculateHelp();
                    return 0;
                }
                else
                {
                    Console.WriteLine($"Error: Unknown option '{arg}' for calculate command.");
                    return 1;
                }
                i++;
            }
        }

        // ── Execute ─────────────────────────────────────────────────
        double result = options.Operation switch
        {
            "add" or "+" => options.Left + options.Right,
            "subtract" or "sub" or "-" => options.Left - options.Right,
            "multiply" or "mul" or "*" or "x" => options.Left * options.Right,
            "divide" or "div" or "/" => options.Right == 0
                ? throw new DivideByZeroException("Cannot divide by zero.")
                : options.Left / options.Right,
            _ => throw new ArgumentException($"Unknown operation '{options.Operation}'.")
        };

        Console.WriteLine($"Result: {Math.Round(result, options.Precision)}");
        return 0;
    }

    // ═══════════════════════════════════════════════════════════════
    //  ECHO COMMAND
    //  Usage: echo <message> [--upper] [--prefix <text>]
    //  Demonstrates: collecting all remaining args as a single value
    // ═══════════════════════════════════════════════════════════════
    private static int HandleEcho(string[] args)
    {
        if (args.Length == 0 || args[0] is "--help" or "-h")
        {
            Console.WriteLine("Usage: echo <message> [--upper] [--prefix <text>]");
            Console.WriteLine("  message   The text to echo (can contain spaces, quote it)");
            Console.WriteLine("  --upper   Convert to uppercase");
            Console.WriteLine("  --prefix  Add a prefix to the output");
            return 0;
        }

        bool upper = false;
        string? prefix = null;
        var messageParts = new List<string>();

        int i = 0;
        while (i < args.Length)
        {
            string arg = args[i];

            if (arg is "--upper" or "-u")
            {
                upper = true;
            }
            else if (arg is "--prefix" or "-p")
            {
                if (i + 1 >= args.Length) { Console.WriteLine("Error: --prefix requires a value."); return 1; }
                prefix = args[++i];
            }
            else
            {
                // Everything else is part of the message
                messageParts.Add(arg);
            }
            i++;
        }

        string message = string.Join(" ", messageParts);
        if (upper) message = message.ToUpperInvariant();
        if (!string.IsNullOrEmpty(prefix)) message = $"[{prefix}] {message}";

        Console.WriteLine(message);
        return 0;
    }

    private static int HandleUnknownCommand(string command)
    {
        Console.WriteLine($"Error: Unknown command '{command}'.");
        Console.WriteLine("Run with --help to see available commands.");
        return 1;
    }

    public void PrintHelp()
    {
        var sb = new StringBuilder();
        sb.AppendLine("╔══════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║           MyConsoleApp - CLI Command Router Demo             ║");
        sb.AppendLine("╚══════════════════════════════════════════════════════════════╝");
        sb.AppendLine();
        sb.AppendLine("USAGE: MyConsoleApp <command> [options]");
        sb.AppendLine();
        sb.AppendLine("COMMANDS:");
        sb.AppendLine("  greet <name>              Greet someone by name");
        sb.AppendLine("    --style, -s <style>     Greeting style: formal | casual (default: casual)");
        sb.AppendLine("    --count, -c <N>         Repeat N times (default: 1)");
        sb.AppendLine("    --shout                 Convert greeting to uppercase");
        sb.AppendLine();
        sb.AppendLine("  calculate <l> <op> <r>   Perform a calculation");
        sb.AppendLine("    --left, -l <number>     Left operand");
        sb.AppendLine("    --right, -r <number>    Right operand");
        sb.AppendLine("    --operation, --op, -o   Operation: add | subtract | multiply | divide");
        sb.AppendLine("    --precision, -p <N>     Decimal places (default: 2)");
        sb.AppendLine();
        sb.AppendLine("  echo <message>            Echo a message back");
        sb.AppendLine("    --upper, -u             Convert to uppercase");
        sb.AppendLine("    --prefix, -p <text>      Add a prefix");
        sb.AppendLine();
        sb.AppendLine("GLOBAL OPTIONS:");
        sb.AppendLine("  --help, -h                Show help for a command or this message");
        sb.AppendLine();
        sb.AppendLine("EXAMPLES:");
        sb.AppendLine("  MyConsoleApp greet Alice --style formal --count 3");
        sb.AppendLine("  MyConsoleApp calc 10 add 5 --precision 0");
        sb.AppendLine("  MyConsoleApp calc --left 10 --op multiply --right 3");
        sb.AppendLine("  MyConsoleApp echo \"Hello World\" --upper --prefix INFO");

        Console.WriteLine(sb.ToString());
    }

    private static void PrintCalculateHelp()
    {
        Console.WriteLine("Usage: calculate <l> <op> <r> [--precision N]");
        Console.WriteLine("   or: calculate --left N --op add --right N [--precision N]");
        Console.WriteLine();
        Console.WriteLine("Operations: add (+), subtract (-), multiply (*), divide (/)");
    }
}
