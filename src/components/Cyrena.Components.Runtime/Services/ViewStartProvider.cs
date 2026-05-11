using Cyrena.Contracts;
using Cyrena.Models;

namespace Cyrena.Services
{
    internal class ViewStartProvider : IViewStartProvider
    {
        public IEnumerable<ViewStart> Provide()
        {
            var index = new ViewStart()
            {
                Href = "/",
                Title = "Default",
                Description = "Start a conversation with any of your assistants."
            };
            return [index];
        }
    }
}
