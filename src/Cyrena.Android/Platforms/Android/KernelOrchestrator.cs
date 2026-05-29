using Cyrena.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using AndroidApp = Android.App.Application;
using AndroidContext = Android.Content.Context;
using AndroidIntent = Android.Content.Intent;

namespace Cyrena.Platforms.Android
{
    internal class KernelOrchestrator : IStartupTask
    {
        private readonly AndroidContext _context;
        private readonly IKernelController _controller;
        public KernelOrchestrator(AndroidContext context, IKernelController controller)
        {
            _context = context;
            _controller = controller;
        }

        public int Order => 10;
        private bool _isForgroundRunning { get; set; } = false;

        public Task RunAsync(CancellationToken cancellationToken = default)
        {
            _controller.OnChatLoaded((cfg) =>
            {
                if (_controller.ActiveKernels.Count > 0 && !_isForgroundRunning)
                {
                    var intent = new AndroidIntent(_context, typeof(IterationForegroundService));
                    _context.StartForegroundService(intent);
                    _isForgroundRunning = true;
                }
            });

            _controller.OnChatUnload((cfg) =>
            {
                if(_isForgroundRunning && _controller.ActiveKernels.Count == 0)
                {
                    var intent = new AndroidIntent(_context, typeof(IterationForegroundService));
                    _context.StopService(intent);
                    _isForgroundRunning=false;
                }
            });
            return Task.CompletedTask;
        }
    }
}
