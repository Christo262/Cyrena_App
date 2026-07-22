using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Android.Components
{
    public partial class Routes
    {
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            var status = await Permissions.RequestAsync<Permissions.PostNotifications>();
        }
    }
}
