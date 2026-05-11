using Cyrena.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Components.Shared
{
    public partial class Shortcuts
    {
        [Inject] private IServiceProvider _services { get; set; } = default!;

        private IEnumerable<IShortcut> _models = Enumerable.Empty<IShortcut>();
        private IEnumerable<string> _categories = [];

        protected override void OnInitialized()
        {
            _models = _services.GetServices<IShortcut>();
            _categories = _models.Select(x => x.Category).Distinct().OrderBy(x => x);
        }
    }
}
