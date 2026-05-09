using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Synthesis.Models;
using Cyrena.Synthesis.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Cyrena.Synthesis.Components.Shared
{
    public partial class Request : IResultDialog
    {
        [Parameter] public ModelCapabilityRequest Model { get; set; } = default!;
        [Inject] private IKernelController _kernels { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;

        private EditContext _context = default!;
        private ChatConfiguration _config = default!;
        protected override void OnInitialized()
        {
            _config = new ChatConfiguration()
            {
                Id = Guid.NewGuid().ToString(),
                Title = Model.Title,
                AssistantModeId = SynthesisOptions.AssistantId,
            };
            _config[ChatConfiguration.Icon] = "bi bi-wrench-adjustable-circle";
            _config[ChatConfiguration.Group] = "Capability Builder";
            _context = new EditContext(_config);
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
            {
                var kernel = await _kernels.Create(_config);
                var its = kernel.GetRequiredService<IIterationService>();
                var cfg = kernel.GetRequiredService<IChatMessageService>();
                its.Input = Model.Instruction;
                its.Iterate(cfg.Options.User, kernel);
                _nav.NavigateTo($"converse/{_config.Id}");
            }
            return valid;
        }
    }
}
