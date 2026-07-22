using System.ComponentModel.DataAnnotations;
using Cyrena.Contracts;
using Cyrena.Dotnet.Options;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Cyrena.VisualStudio.Components.Shared;

public partial class Configure
{
    [Inject] private IDialogService _dialog { get; set; } = null!;
    [Inject] private IFileDialog _file { get; set; } = null!;
    [Inject] private ISnackbar _snackbar { get; set; } = null!;
    [Parameter] public ChatConfiguration Model { get; set; } = null!;
    [Parameter] public string[] Filter { get; set; } = ["csproj"];
    [Parameter] public bool IsSolutionFile { get; set; } = false;
    [CascadingParameter] private IMudDialogInstance? _mudDialog { get; set; }
    private ConfigureModel _model = null!;
    private MudForm _form = null!;
    
    protected override void OnInitialized()
    {
        _model = new ConfigureModel()
        {
            Title = Model.Title,
            ConnectionId = Model.ConnectionId,
            ProjectFilePath = IsSolutionFile ? Model[DotnetOptions.SolutionFilePath]:Model[DotnetOptions.ProjectFilePath],
        };
    }
    
    private async Task ChooseFile()
    {
        string filterName = string.Join('|', Filter.Select(x => $".{x}"));
        var filters = Filter.Select(x => $".{x}");
        var f = await _file.OpenAsync($"Choose .{filterName}", (filterName, filters.ToArray()));
        if (f != null)
            _model.ProjectFilePath = f;
    }
    
    private async Task Submit()
    {
        await _form.ValidateAsync();
        if (!_form.IsValid)
            return;

        if (!File.Exists(_model.ProjectFilePath))
        {
            _snackbar.Add($"{(IsSolutionFile ? "Solution" :"Project")} file not found", Severity.Error);
            return;
        }

        Model.Title = _model.Title;
        Model.ConnectionId = _model.ConnectionId ?? string.Empty;
        Model.WorkingDirectory = new FileInfo(_model.ProjectFilePath).DirectoryName ?? string.Empty;
        if(IsSolutionFile)
            Model[DotnetOptions.SolutionFilePath] = _model.ProjectFilePath;
        else
            Model[DotnetOptions.ProjectFilePath] = _model.ProjectFilePath;
        _mudDialog?.Close(DialogResult.Ok(true));
    }
    
    private void Cancel() => _mudDialog?.Cancel();
}

internal class ConfigureModel
{
    [Required]
    public string? Title { get; set; }
    [Required]
    public string? ConnectionId { get; set; }
    [Required]
    public string? ProjectFilePath { get; set; }
}