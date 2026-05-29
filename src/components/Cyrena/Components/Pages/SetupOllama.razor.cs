using Cyrena.Contracts;
using Cyrena.Persistence.Contracts;
using Cyrena.Runtime.Ollama.Models;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Components.Pages
{
    public partial class SetupOllama
    {
        [Inject] private IStore<OllamaConnectionInfo> _store { get; set; } = default!;
        [Inject] private ISetupService _setup { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;

        private OllamaConnectionInfo _model =default!;

        protected override void OnInitialized()
        {
            _model = new OllamaConnectionInfo();
            _model.NumPredict = 8192;
            _model.NumContext = 16384;
        }

        private async Task Submit()
        {
            _model.Id = Guid.NewGuid().ToString();
            await _store.AddAsync(_model);
            await _setup.SetDefaultConnectionId(_model.Id);
            _nav.NavigateTo("app-setup/complete");
            await Task.Delay(100);
        }
    }
}
