using Cyrena.Contracts;
using Cyrena.Synthesis.Contracts;
using Cyrena.Synthesis.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Cyrena.Synthesis.Services
{
    internal class CapabilityContext : ICapabilityContext
    {
        private readonly ICapabilityPermissionService _permissionService;
        private readonly IDisplayService _displayService;
        private readonly SemaphoreSlim _permissionModalLock = new(1, 1);
        public CapabilityContext(ICapabilityPermissionService permissionService, IDisplayService displayService)
        {
            _permissionService = permissionService;
            _displayService = displayService;
        }

        public DynamicCapability? Current { get; private set; }

        public void SetCurrent(DynamicCapability? capability)
        {
            Current = capability;
        }

        public async Task<bool> RequestPermissionAsync(DynamicCapability capability, CapabiliyPermissionDescriptor dsc)
        {
            if (await _permissionService.HasPermissionAsync(capability.Id, dsc.Permission))
                return true;

            await _permissionModalLock.WaitAsync();

            try
            {
                var result = await _displayService.ShowModal("Permission Required", $"'{capability.Title}' requires {dsc.Permission}: <br> {dsc.Description}", new BootstrapBlazor.Components.ResultDialogOption()
                {
                    Size = BootstrapBlazor.Components.Size.Medium,
                    ButtonYesText = "Allow",
                    ButtonNoText = "Deny",
                });
                if (result == BootstrapBlazor.Components.DialogResult.Yes)
                {
                    await _permissionService.GrantPermissionAsync(capability.Id, dsc.Permission);
                    return true;
                }
                return false;
            }
            finally
            {
                await Task.Delay(2000); //breathing room on UI thread
                _permissionModalLock.Release();
            }
        }

        public async Task<bool> HasPermissionAsync(DynamicCapability capability, string permission)
        {
            return await _permissionService.HasPermissionAsync(capability.Id, permission);
        }

        public async Task<bool> HasPermissionAsync(DynamicCapability capability, CapabiliyPermissionDescriptor dsc)
        {
            return await _permissionService.HasPermissionAsync(capability.Id, dsc.Permission);
        }
    }
}
