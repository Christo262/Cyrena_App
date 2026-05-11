using Cyrena.Contracts;
using Cyrena.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Platform.Tests.Services
{
    internal class ViewStartProvider : IViewStartProvider
    {
        public IEnumerable<ViewStart> Provide()
        {
            var v = new ViewStart()
            {
                Href = "/platform-test-start",
                Title = "Test",
                Description = "Testing view start"
            };
            return [v];
        }
    }
}
