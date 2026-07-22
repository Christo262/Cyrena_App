using CommunityToolkit.Mvvm.Input;
using MauiApp2.Models;

namespace MauiApp2.PageModels;

public interface IProjectTaskPageModel
{
    IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
    bool IsBusy { get; }
}