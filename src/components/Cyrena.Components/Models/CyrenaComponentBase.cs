using Microsoft.AspNetCore.Components;

namespace Cyrena.Models
{
    public abstract class CyrenaComponentBase : ComponentBase
    {
        protected override void OnAfterRender(bool firstRender)
        {
            if (!firstRender) return;
            OnFirstRender();
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if(!firstRender)return Task.CompletedTask;
            return OnFirstRenderAsync();
        }

        public virtual void OnFirstRender() { }
        public virtual Task OnFirstRenderAsync() { return Task.CompletedTask; }
    }
}
