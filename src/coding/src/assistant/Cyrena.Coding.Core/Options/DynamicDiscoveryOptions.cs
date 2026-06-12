using Cyrena.Coding.Models;

namespace Cyrena.Coding.Options;

public class DynamicDiscoveryOptions
{
    public Action<DevelopPlan>? Initialization { get; set; }
}