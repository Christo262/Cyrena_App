using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace Cyrena.Shell;

public partial class ErrorWindow : Window
{
    private readonly string _errorDetails;

    public ErrorWindow()
    {
        InitializeComponent();
        _errorDetails = string.Empty;
    }

    public ErrorWindow(Exception exception)
    {
        InitializeComponent();

        _errorDetails = exception.ToString();
        ErrorText.Text = _errorDetails;
    }

    public ErrorWindow(string errorDetails)
    {
        InitializeComponent();

        _errorDetails = errorDetails;
        ErrorText.Text = errorDetails;
    }

    private async void CopyDetails_Click(object? sender, RoutedEventArgs e)
    {
        IClipboard? clipboard = TopLevel.GetTopLevel(this)?.Clipboard;

        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(_errorDetails);
        }
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }
}