using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Android.Components
{
    public partial class Routes
    {
        //[Inject] private IServiceProvider _services { get; set; } = default!;
        //private CancellationTokenSource? _cts;
        //protected override async Task OnAfterRenderAsync(bool firstRender)
        //{
        //    if (!firstRender) return;
        //    _cts = new CancellationTokenSource();
        //    var hosts = _services.GetServices<IHostedService>();
        //    foreach (var item in hosts)
        //        await item.StartAsync(_cts.Token);
        //}

        //public async ValueTask DisposeAsync()
        //{
        //    if(_cts != null)
        //    {
        //        var hosts = _services.GetServices<IHostedService>();
        //        foreach (var item in hosts)
        //            await item.StopAsync(_cts.Token);
        //        await _cts.CancelAsync();
        //        _cts.Dispose();
        //    }
        //}
    }
}
