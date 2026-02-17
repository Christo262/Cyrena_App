using Cyrena.Contracts;

namespace Cyrena.Extensions
{
    public static class ChatMessageServiceExtensions
    {
        public static Task LogInfo(this IChatMessageService service, string? message)
        {
            return service.AddMessage(service.Options.LogInfo, message);
        }

        public static Task LogSuccess(this IChatMessageService service, string? message)
        {
            return service.AddMessage(service.Options.LogSuccess, message);
        }

        public static Task LogWarn(this IChatMessageService service, string? message)
        {
            return service.AddMessage(service.Options.LogWarn, message);
        }

        public static Task LogError(this IChatMessageService service, string? message)
        {
            return service.AddMessage(service.Options.LogError, message);
        }

        public static Task AddSystemMessage(this IChatMessageService service, string? message)
        {
            return service.AddMessage(service.Options.System, message);
        }

        public static Task AddAssistantMessage(this IChatMessageService service, string? message)
        {
            return service.AddMessage(service.Options.Assistant, message);
        }

        public static Task AddUserMessage(this IChatMessageService service, string? message)
        {
            return service.AddMessage(service.Options.User, message);
        }

        public static Task AddToolMessage(this IChatMessageService service, string? message)
        {
            return service.AddMessage(service.Options.Tool, message);
        }
    }
}
