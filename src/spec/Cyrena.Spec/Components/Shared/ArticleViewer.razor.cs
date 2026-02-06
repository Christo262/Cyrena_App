using BootstrapBlazor.Components;
using Cyrena.Spec.Contracts;
using Cyrena.Spec.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Spec.Components.Shared
{
    public partial class ArticleViewer
    {
        private ISpecsService _specs = default!;
        [Inject] private DialogService _dialog { get; set; } = default!;
        protected override void OnInitialized()
        {
            _specs = Context.Kernel.Services.GetRequiredService<ISpecsService>();
        }

        private bool _open { get;set;  }
        public void Toggle() => _open = !_open;

        private string _dsc = "Engineering Memory is the project’s long-term technical brain. It stores structured documentation about architecture, services, APIs, integration rules, and design decisions.";

        private async Task Edit(Article model)
        {
            var rf = await _dialog.ShowModal<ArticleForm>(new ResultDialogOption()
            {
                Title = "Edit Article",
                Size = Size.Large,
                ButtonYesText = "Submit",
                ButtonNoText = "Cancel",
                ComponentParameters = new() { { "Model", model} }
            });
            if(rf == DialogResult.Yes)
            {
                _specs.Update(model);
            }
        }

        private async Task Edit(ContextMenuItem item, object obj)
        {
            if(obj is Article a) 
                await Edit(a);
        }

        private async Task Create()
        {
            var model = new Article();
            var rf = await _dialog.ShowModal<ArticleForm>(new ResultDialogOption()
            {
                Title = "Create Article",
                Size = Size.Large,
                ButtonYesText = "Submit",
                ButtonNoText = "Cancel",
                ComponentParameters = new() { { "Model", model } }
            });
            if (rf == DialogResult.Yes)
            {
                _specs.Create(model);
            }
        }

        private async Task Delete(Article model)
        {
            var r = await _dialog.ShowModal("Delete Article", $"Are you sure you want to delete {model.Title}?", new ResultDialogOption()
            {
                Size = Size.Medium
            });
            if (r == DialogResult.Yes) 
                _specs.Delete(model);
        }

        private async Task Delete(ContextMenuItem item, object obj)
        {
            if (obj is Article a)
                await Delete(a);
        }
    }
}
