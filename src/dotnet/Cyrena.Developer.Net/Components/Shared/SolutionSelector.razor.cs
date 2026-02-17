using Cyrena.Contracts;
using Cyrena.Developer.Contracts;

namespace Cyrena.Developer.Components.Shared
{
    public partial class SolutionSelector
    {
        private ISolutionController _sln = default!;
        private IIterationService _its = default!;
        private string _sln_id { get; set; } = default!;
        protected override void OnInitialized()
        {
            _sln = Kernel.GetRequiredService<ISolutionController>();
            _its = Kernel.GetRequiredService<IIterationService>();
            _its.OnIterationStart(e => this.InvokeAsync(StateHasChanged));
            _its.OnIterationEnd(e => this.InvokeAsync(StateHasChanged));
            _sln_id = _sln.Current.Id;
            _sln.OnProjectChange(p =>
            {
                _sln_id = _sln.Current.Id;
                this.InvokeAsync(StateHasChanged);
            });
        }

        private async Task OnValueChange()
        {
            if (_its.Inferring) return;
            var proj = _sln.GetValidProjects().FirstOrDefault(x => x.Id == _sln_id);
            if (proj != null) 
                await _sln.SetTargetProject(proj);
        }
    }
}
