using Cyrena.Coding.Models;
using Cyrena.VisualStudio.Models;

namespace Cyrena.VisualStudio.Contracts;

public interface IProjHandler
{
    string Filter { get; }
    string Title { get; }
    string PromptId { get; }
    string Description { get; }
    Tools Tools { get; }
    
    void Initialize(DevelopPlan plan);
}