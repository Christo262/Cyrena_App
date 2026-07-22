using Cyrena.Coding.Models;

namespace Cyrena.Coding.Contracts;

public interface IDynamicPlanInitializer
{
    void Initialize();
    void RunIndex();
}