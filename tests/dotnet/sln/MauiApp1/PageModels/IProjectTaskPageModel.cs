using CommunityToolkit.Mvvm.Input;
using MauiApp1.Models;

namespace MauiApp1.PageModels;

public interface IProjectTaskPageModel
{
	IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
	bool IsBusy { get; }
}