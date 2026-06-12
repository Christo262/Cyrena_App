using Cyrena.Coding.Models;

namespace Cyrena.VisualStudio.Contracts;

public interface IProjHandler
{
    string Filter { get; }
    string Title { get; }
    string PromptId { get; }
    string Description { get; }
    
    void Initialize(DevelopPlan plan);
}