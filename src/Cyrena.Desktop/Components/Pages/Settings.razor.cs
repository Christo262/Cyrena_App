using Cyrena.Options;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Desktop.Components.Pages
{
    public partial class Settings
    {
        [Inject] private ComponentOptions _options { get; set; } = default!;
    }
}
