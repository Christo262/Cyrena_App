using Cyrena.Contracts;
using Cyrena.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cyrena.Runtime.Services
{
    /// <summary>
    /// SK cannot serialize <see cref="Microsoft.SemanticKernel.FileReferenceContent"/>. 
    /// This transforms any of them into text so model can refer to them
    /// </summary>
    internal class FileReferenceContentTransformer : ConversationHistoryTransformer
    {
        public override Task<ChatHistory> TransformPreIterationHistory(ChatHistory history)
        {
#pragma warning disable SKEXP0110
            var msgs = history.Where(x => x.Items.Any(t => t is FileReferenceContent));
            foreach (var item in msgs)
            {
                var targets = item.Items.Where(x => x is FileReferenceContent);
                for(int i = 0; i < targets.Count(); i++)
                {
                    FileReferenceContent target = (FileReferenceContent)targets.ElementAt(i);
                    var index = item.Items.IndexOf(target);
                    var name = target.Metadata?.ContainsKey("name") == true ? target.Metadata["name"] : "Unknown";
                    item.Items[index] = target.ToTextContent();
                }
            }
            return Task.FromResult(history);
#pragma warning restore SKEXP0110
        }
    }
}
