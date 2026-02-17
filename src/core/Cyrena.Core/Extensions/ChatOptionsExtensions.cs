using Cyrena.Options;
using Microsoft.SemanticKernel;

namespace Cyrena.Extensions
{
    public static class ChatOptionsExtensions
    {
        public static bool IsDisplayContent(this ChatOptions options, ChatMessageContent content)
        {
            return content.Role == options.User || content.Role == options.Assistant 
                || content.Role == options.LogInfo 
                || content.Role == options.LogSuccess
                || content.Role == options.LogWarn
                || content.Role == options.LogError;
        }

        public static bool IsKernelContent(this ChatOptions options, ChatMessageContent content)
        {
            return content.Role == options.User
                || content.Role == options.System
                || content.Role == options.Assistant
                || content.Role == options.Tool;
        }
    }
}
