using BlazorApp.Components.Pages;

namespace BlazorApp.Contracts
{
    public interface IBugReportEmailService
    {
        Task SendBugReportAsync(BugReport.BugReportModel bugReport);
    }
}
