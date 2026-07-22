using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cyrena.Runtime.Services
{
    internal class PromptManager : IPromptManager
    {
        public PromptManager()
        {
            _prompts = new List<Prompt>();
        }

        public IReadOnlyList<Prompt> Prompts => _prompts.AsReadOnly();
        private readonly List<Prompt> _prompts;

        public string AddPrompt(int order, string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;
            var p = new Prompt()
            {
                Order = order,
                Content = content,
            };
            _prompts.Add(p);
            return p.Id;
        }

        public void RemovePrompt(string id)
        {
            var p = _prompts.FirstOrDefault(p => p.Id == id);
            if(p != null)
                _prompts.Remove(p);
        }

        public void UpdatePrompt(string id, string content)
        {
            if(string.IsNullOrEmpty(content)) return;
            var p = _prompts.FirstOrDefault(p => p.Id == id);
            if (p == null)
                return;
            var np = new Prompt()
            {
                Id = p.Id,
                Content = content,
                Order = p.Order,
            };
            _prompts.Remove(p);
            _prompts.Add(np);
        }
    }
}
