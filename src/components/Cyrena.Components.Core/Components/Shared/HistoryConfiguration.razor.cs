using Cyrena.Models;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Components.Shared
{
    public partial class HistoryConfiguration
    {
        [Parameter] public ChatConfiguration Model { get; set; } = default!;
    }
}
