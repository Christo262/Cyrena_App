using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Persistence.Contracts;
using Cyrena.Runtime.OpenAI.Models;
using Cyrena.Runtime.OpenAI.Options;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.Components.Pages
{
    public partial class SetupOpenAI
    {
        [Inject] private ISettingsService _settings { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;
        [Inject] private IStore<OpenAIModel> _store { get; set; } = default!;
        [Inject] private ISetupService _setup { get; set; } = default!;
        [CascadingParameter]
        public TabItem? Item { get; set; }
        [CascadingParameter]
        public Tab? Parent { get; set; }
        private OpenAISettingsViewModel _model = new();

        protected override void OnInitialized()
        {
            var options = _settings.Read<OpenAIOptions>(OpenAIOptions.Key);
            _model.ApiKey = options?.ApiKey;
            _model.ModelId = "gpt-5";
            _model.DisplayName = "GPT5";
        }

        private async Task Submit()
        {
            var options = _settings.Read<OpenAIOptions>(OpenAIOptions.Key) ?? new OpenAIOptions();
            options.ApiKey = _model.ApiKey;
            _settings.Save(OpenAIOptions.Key, options);

            var model = new OpenAIModel()
            {
                Id = Guid.NewGuid().ToString(),
                DisplayName = _model.DisplayName,
                ModelId = _model.ModelId,
            };
            await _store.AddAsync(model);
            await _setup.SetDefaultConnectionId(model.Id);
            _nav.NavigateTo("app-setup/complete");
            await Task.Delay(100);
            if (Item != null && Parent != null)
                await Parent.RemoveTab(Item);
        }
    }

    internal class OpenAISettingsViewModel
    {
        [Required]
        public string? ApiKey { get; set; }
        [Required]
        public string? ModelId { get; set; }
        [Required]
        public string? DisplayName { get; set; }
    }
}
