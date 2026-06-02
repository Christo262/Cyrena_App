# CLI Reference

Cyréna provides a `cyrena` command for running CLI commands.

If Cyréna has been added to your `PATH`, commands can be run from any directory:

```bash
cyrena help
```

If Cyréna is not on your `PATH`, commands must be run from the Cyréna installation directory using the local command:

```bash
./cyrena help
```

## Running a Command

When `cyrena` is available on your `PATH`:

```bash
cyrena help
```

From the Cyréna installation directory:

```bash
./cyrena help
```

On Windows, the same command is provided by `cyrena.cmd`:

```powershell
cyrena help
```

Or, from the installation directory:

```powershell
.\cyrena help
```

## Getting Help for a Specific Command

To view help for a specific command, pass the command name to `--help`:

```bash
cyrena --help set
```

From the Cyréna installation directory:

```bash
./cyrena --help set
```

On Windows:

```powershell
cyrena --help set
```

Or, from the installation directory:

```powershell
.\cyrena --help set
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

When `cyrena` is available on your `PATH`:

```bash
cyrena set --port 5000 --launch-window true
```

From the Cyréna installation directory:

```bash
./cyrena set --port 5000 --launch-window true
```

On Windows, from the installation directory:

```powershell
.\cyrena set --port 5000 --launch-window true
```

## `kill` Command

The `kill` command attempts to stop Cyréna's background process and close any open Shell windows.

This command includes validation to ensure that only the user who started Cyréna can stop the running process.

Use this command if you cannot access the tray icon and need to stop Cyréna manually.

When `cyrena` is available on your `PATH`:

```bash
cyrena kill
```

From the Cyréna installation directory:

```bash
./cyrena kill
```

On Windows, from the installation directory:

```powershell
.\cyrena kill
```
