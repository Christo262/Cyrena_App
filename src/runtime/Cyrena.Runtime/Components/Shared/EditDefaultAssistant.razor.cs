using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.Runtime.Components.Shared
{
    public partial class EditDefaultAssistant : IResultDialog
    {
        [Parameter] public ChatConfiguration Model { get; set; } = default!;
        [Inject] private IServiceProvider _services { get; set; } = default!;
        private List<ConnectionInfo> _models { get; set; } = new();

        private DefaultChatViewModel _model  = default!;
        private EditContext _context { get; set; } = default!;

        protected override void OnInitialized()
        {
            _model = new DefaultChatViewModel()
            {
                ConnectionId = Model.ConnectionId,
            };
            _context = new EditContext(_model);
        }

        Task IResultDialog.OnClose(DialogResult result)
        {
            return Task.CompletedTask;
        }

        async Task<bool> IResultDialog.OnClosing(DialogResult result)
        {
            if (result != DialogResult.Yes) return true;
            var valid = _context.Validate();
            if (valid)
                Model.ConnectionId = _model.ConnectionId!;
            return valid;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            var providers = _services.GetServices<IConnectionProvider>();
            foreach (var item in providers)
            {
                var infos = await item.ListConnectionsAsync();
                _models.AddRange(infos);
            }
            this.StateHasChanged();
        }
    }

    internal class DefaultChatViewModel
    {
        [Required]
        public string? ConnectionId { get; set; }
    }
}
