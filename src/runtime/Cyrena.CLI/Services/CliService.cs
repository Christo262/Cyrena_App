using System.Reflection;
using Cyrena.CLI.Attributes;
using Cyrena.CLI.Contracts;
using Cyrena.CLI.Models;

namespace Cyrena.CLI.Services;

/// <summary>
/// Service for discovering, registering, and executing CLI commands.
/// Only classes marked with <see cref="CliSurfaceAttribute"/> are discovered.
/// </summary>
public sealed class CliService : ICliService
{
    private readonly Dictionary<string, CliCommandDescriptor> _commands = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public void RegisterCommandsFromAssembly(Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<CliSurfaceAttribute>() != null);

        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);
            RegisterCommandsFromType(type, instance);
        }
    }

    /// <inheritdoc/>
    public void RegisterCommandsFromType(Type commandType, object? instance = null)
    {
        if (!commandType.IsClass)
            throw new ArgumentException($"Type '{commandType.Name}' must be a class.", nameof(commandType));

        if (commandType.GetCustomAttribute<CliSurfaceAttribute>() == null)
            throw new ArgumentException($"Type '{commandType.Name}' must be marked with [CliSurface] attribute.", nameof(commandType));

        var methods = commandType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

        foreach (var method in methods)
        {
            var descriptor = CliCommandDescriptor.Create(method, instance);
            if (descriptor != null)
            {
                if (_commands.ContainsKey(descriptor.Name))
                    throw new InvalidOperationException($"Command '{descriptor.Name}' is already registered.");

                _commands[descriptor.Name] = descriptor;
            }
        }
    }

    /// <inheritdoc/>
    public CliExecutionResult Execute(string commandName, string[] args)
    {
        // No command specified - allow boot to continue
        if (string.IsNullOrEmpty(commandName))
        {
            return CliExecutionResult.NoCommand();
        }

        // Handle built-in help command
        if (commandName == "help" || commandName == "--help" || commandName == "-h" || args.Contains("--help") || args.Contains("-h"))
        {
            string? targetCommand = null;
            
            if (commandName == "help")
            {
                // help <commandName>
                targetCommand = args.FirstOrDefault(a => !a.StartsWith("--") && !a.StartsWith("-"));
            }
            else if (commandName == "--help" || commandName == "-h")
            {
                // --help <commandName> or -h <commandName>
                targetCommand = args.FirstOrDefault(a => !a.StartsWith("--") && !a.StartsWith("-"));
            }
            else
            {
                // <commandName> --help or <commandName> -h
                // commandName is the actual command, args contains --help/-h
                targetCommand = commandName;
            }
            
            Console.WriteLine(GetHelpText(targetCommand));
            return CliExecutionResult.CommandExecuted();
        }

        if (!_commands.TryGetValue(commandName, out var command))
        {
            return CliExecutionResult.UnknownCommand(commandName);
        }

        try
        {
            var parameters = ParseParameters(command, args);
            var result = command.Method.Invoke(command.Target, parameters);

            // Handle void methods - default to CommandExecuted
            if (result == null)
            {
                return CliExecutionResult.CommandExecuted();
            }

            // Handle direct CliExecutionResult return
            if (result is CliExecutionResult executionResult)
            {
                return executionResult;
            }

            // Handle Task return types (async methods)
            if (result is Task task)
            {
                // Block on the task - acceptable in CLI pre-boot context
                task.GetAwaiter().GetResult();

                // Check if it's Task<CliExecutionResult>
                if (task.GetType().IsGenericType && 
                    task.GetType().GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var resultProperty = task.GetType().GetProperty("Result");
                    if (resultProperty != null)
                    {
                        var taskResult = resultProperty.GetValue(task);
                        if (taskResult is CliExecutionResult asyncResult)
                        {
                            return asyncResult;
                        }
                    }
                }

                // Task completed without result - default to CommandExecuted
                return CliExecutionResult.CommandExecuted();
            }

            // Unknown return type - default to CommandExecuted
            return CliExecutionResult.CommandExecuted();
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            Console.Error.WriteLine($"Error executing command '{commandName}': {ex.InnerException.Message}");
            return CliExecutionResult.Stop(exitCode: 1, message: ex.InnerException.Message);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error executing command '{commandName}': {ex.Message}");
            return CliExecutionResult.Stop(exitCode: 1, message: ex.Message);
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<CliCommandDescriptor> GetRegisteredCommands()
    {
        return _commands.Values.ToList().AsReadOnly();
    }

    /// <inheritdoc/>
    public CliCommandDescriptor? GetCommand(string commandName)
    {
        return _commands.TryGetValue(commandName, out var cmd) ? cmd : null;
    }

    /// <inheritdoc/>
    public string GetHelpText(string? commandName = null)
    {
        if (!string.IsNullOrEmpty(commandName))
        {
            return GetCommandHelpText(commandName!);
        }

        return GetAllCommandsHelpText();
    }

    private string GetCommandHelpText(string commandName)
    {
        if (!_commands.TryGetValue(commandName, out var command))
        {
            return $"Unknown command: {commandName}";
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Usage: {command.Name} [options]");
        
        if (!string.IsNullOrEmpty(command.Description))
        {
            sb.AppendLine();
            sb.AppendLine($"Description:");
            sb.AppendLine($"  {command.Description}");
        }

        if (command.Parameters.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"Options:");
            foreach (var param in command.Parameters)
            {
                var required = param.Required ? " (required)" : "";
                var defaultVal = param.DefaultValue != null ? $" [default: {param.DefaultValue}]" : "";
                var typeHint = GetTypeHint(param.ParameterType);
                
                sb.AppendLine($"  --{param.Name}{typeHint}{required}{defaultVal}");
                if (!string.IsNullOrEmpty(param.Description))
                {
                    sb.AppendLine($"      {param.Description}");
                }
            }
        }

        return sb.ToString().TrimEnd();
    }

    private string GetAllCommandsHelpText()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Available Commands:");
        sb.AppendLine();

        var sortedCommands = _commands.Values.OrderBy(c => c.Name).ToList();
        
        foreach (var command in sortedCommands)
        {
            var desc = !string.IsNullOrEmpty(command.Description) ? $" - {command.Description}" : "";
            sb.AppendLine($"  {command.Name}{desc}");
        }

        sb.AppendLine();
        sb.AppendLine("Use '--help <command>' for more information on a specific command.");

        return sb.ToString().TrimEnd();
    }

    private static string GetTypeHint(Type type)
    {
        if (type == typeof(bool))
            return "";
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return $"<{Nullable.GetUnderlyingType(type)!.Name}>";
        if (type.IsArray)
            return $"<{type.GetElementType()!.Name}[]>";
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            return $"<List<{type.GenericTypeArguments[0].Name}>>";
        return $"<{type.Name}>";
    }

    private object?[] ParseParameters(CliCommandDescriptor command, string[] args)
    {
        var result = new object?[command.Parameters.Count];
        var argDict = ParseArguments(args);

        for (int i = 0; i < command.Parameters.Count; i++)
        {
            var param = command.Parameters[i];
            
            if (argDict.TryGetValue(param.Name, out var value))
            {
                result[i] = ConvertValue(value, param);
            }
            else if (param.DefaultValue != null)
            {
                result[i] = param.DefaultValue;
            }
            else if (param.Required)
            {
                throw new ArgumentException($"Required parameter '--{param.Name}' is missing for command '{command.Name}'.");
            }
            else
            {
                result[i] = GetDefaultValue(param.ParameterType);
            }
        }

        return result;
    }

    private Dictionary<string, string> ParseArguments(string[] args)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            
            if (!arg.StartsWith("--"))
                continue;

            var parts = arg[2..].Split('=', 2);
            var key = parts[0];
            var value = parts.Length > 1 ? parts[1] : "true";

            // Handle quoted values
            if (value.StartsWith('"') && value.EndsWith('"') && value.Length > 1)
                value = value[1..^1];

            dict[key] = value;
        }

        return dict;
    }

    private object? ConvertValue(string value, CliParameterDescriptor param)
    {
        var targetType = param.ParameterType;

        // Handle nullable types
        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            targetType = Nullable.GetUnderlyingType(targetType)!;
        }

        // Handle multi-value parameters
        if (param.IsMultiValue)
        {
            return ConvertMultiValue(value, param);
        }

        // Handle boolean flags
        if (targetType == typeof(bool))
            return bool.Parse(value);

        // Handle enum types
        if (targetType.IsEnum)
            return Enum.Parse(targetType, value, ignoreCase: true);

        // Handle standard types
        return Convert.ChangeType(value, targetType);
    }

    private object? ConvertMultiValue(string value, CliParameterDescriptor param)
    {
        var elementType = param.ParameterType.IsArray 
            ? param.ParameterType.GetElementType()! 
            : param.ParameterType.GenericTypeArguments[0];

        var values = value.Split(',').Select(v => v.Trim()).ToArray();

        if (param.ParameterType.IsArray)
        {
            var array = Array.CreateInstance(elementType, values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                array.SetValue(Convert.ChangeType(values[i], elementType), i);
            }
            return array;
        }
        else if (param.ParameterType.IsGenericType && 
                 param.ParameterType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var list = Activator.CreateInstance(param.ParameterType)!;
            var addMethod = param.ParameterType.GetMethod("Add")!;
            foreach (var v in values)
            {
                addMethod.Invoke(list, [Convert.ChangeType(v, elementType)]);
            }
            return list;
        }

        return null;
    }

    private object? GetDefaultValue(Type type)
    {
        if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
            return Activator.CreateInstance(type);
        
        return null;
    }
}
