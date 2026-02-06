using BootstrapBlazor.Components;
using Cyrena.ClassLibrary.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.ClassLibrary.Components.Shared
{
    public partial class ClassLibraryConfig : IResultDialog
    {
        [Parameter] public ClassLibraryProject Model { get; set; } = default!;

        private EditContext _context = default!;

        protected override void OnInitialized()
        {
            _context = new EditContext(Model);
        }

        Task IResultDialog.OnClose(DialogResult result)
        {
            return Task.CompletedTask;
        }

        async Task<bool> IResultDialog.OnClosing(DialogResult result)
        {
            if (result != DialogResult.Yes) return true;
            var valid = _context.Validate();
            return valid;
        }

        private async Task PickProject()
        {
            try
            {
                var result = NativeFileDialogs.Net.Nfd.OpenDialog(out var csp);

                if (result == NativeFileDialogs.Net.NfdStatus.Ok)
                {
                    var t = csp;
                    var info = new FileInfo(t);
                    Model.RootDirectory = info.DirectoryName ?? string.Empty;
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
