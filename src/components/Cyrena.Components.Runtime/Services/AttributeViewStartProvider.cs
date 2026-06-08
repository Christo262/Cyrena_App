using Cyrena.Contracts;
using Cyrena.Models;

namespace Cyrena.Services;

public class AttributeViewStartProvider : IViewStartProvider
{
    private readonly IEnumerable<ViewStart> _models;

    public AttributeViewStartProvider(IEnumerable<ViewStart> models)
    {
        _models = models;
    }
    
    public IEnumerable<ViewStart> Provide()
    {
        return _models;
    }
}