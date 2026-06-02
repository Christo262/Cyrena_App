# CLI Reference

Cyréna CLI commands must be run from the Cyréna installation directory.

## Running a Command

```bash
./Cyrena.Shell help
```

On Windows, use:

```bash
./Cyrena.Shell.exe help
```

## Getting Help for a Specific Command

To view help for a specific command, pass the command name to `--help`:

```bash
./Cyrena.Shell --help set
```

On Windows:

```bash
./Cyrena.Shell.exe --help set
```

Example output:

```bash
Usage: set [options]

Options:
  --port<Int32>
      Specify the port number to use for Cyréna's background process.

  --launch-window<Boolean>
      Specifies whether the Shell window should launch on startup.
      Accepted values: true or false.

Press any key to exit
```

## `set` Command

The `set` command changes Cyréna configuration without starting the application.

This is useful when you need to change settings before Cyréna starts, such as when the configured port is already in use.

### Options

| Option                     | Description                                                                                    |
| -------------------------- | ---------------------------------------------------------------------------------------------- |
| `--port<Int32>`            | Sets the port number used by Cyréna's background process.                                      |
| `--launch-window<Boolean>` | Sets whether the Shell window should launch on startup. Accepted values are `true` or `false`. |

### Example

```bash
./Cyrena.Shell set --port 5000 --launch-window true
```

On Windows:

```bash
./Cyrena.Shell.exe set --port 5000 --launch-window true
```

## `kill` Command

The `kill` command attempts to stop Cyréna's background process and close any open Shell windows.

This command includes validation to ensure that only the user who started Cyréna can stop the running process.

Use this command if you cannot access the tray icon and need to stop Cyréna manually.

### macOS / Linux

```bash
./Cyrena.Shell kill
```

### Windows

```bash
./Cyrena.Shell.exe kill
```
