using MyConsoleApp.Contracts;
using MyConsoleApp.Services;

// ═══════════════════════════════════════════════════════════════
//  MyConsoleApp - CLI Command Router Demo
//  This app demonstrates how CLI applications distinguish
//  between commands and parse parameters successfully.
// ═══════════════════════════════════════════════════════════════

// Build a simple DI-like setup
ICliCommandService cli = new CliCommandService();

// Pass all command-line arguments to the router
int exitCode = cli.Execute(args);

// Return the exit code to the OS
Environment.Exit(exitCode);
