using Cyrena.Components.Shared;
using Cyrena.Contracts;
using Cyrena.Models;

namespace Cyrena.Services;

public class DefaultViewStartProvider : IViewStartProvider
{
    public IEnumerable<ViewStart> Provide()
    {
        return [new ViewStart("cyrena.default", typeof(DefaultViewStart), "Start Conversation", null)];
    }
}