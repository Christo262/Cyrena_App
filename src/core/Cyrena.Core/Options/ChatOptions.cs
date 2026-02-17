using Microsoft.SemanticKernel.ChatCompletion;

namespace Cyrena.Options
{
    /// <summary>
    /// Configure roles for the chat
    /// </summary>
    public sealed class ChatOptions
    {
        public ChatOptions()
        {
            System = AuthorRole.System;
            Assistant = AuthorRole.Assistant;
            User = AuthorRole.User;
            Tool = AuthorRole.Tool;
            LogInfo = new AuthorRole("LogInfo");
            LogSuccess = new AuthorRole("LogSuccess");
            LogWarn = new AuthorRole("LogWarn");
            LogError = new AuthorRole("LogError");
        }

        public ChatOptions(AuthorRole system, AuthorRole assistant, AuthorRole user, AuthorRole tool, AuthorRole info, AuthorRole success, AuthorRole warn, AuthorRole error)
        {
            System = system;
            Assistant = assistant;
            User = user;
            Tool = tool;
            LogInfo = info;
            LogSuccess = success;
            LogWarn = warn;
            LogError = error;
        }

        public AuthorRole System { get; }
        public AuthorRole Assistant { get; }
        public AuthorRole User { get; }
        public AuthorRole Tool { get; }
        public AuthorRole LogInfo { get; }
        public AuthorRole LogSuccess { get; }
        public AuthorRole LogWarn { get; }
        public AuthorRole LogError { get; }

        /// <summary>
        /// If logs should be displayed
        /// </summary>
        public bool IncludeLogsInDisplay { get; set; } = true;
        /// <summary>
        /// Autosave messages enabled, default true. Tool calls will not be saved
        /// </summary>
        public bool AutoSave { get; set; } = true;
    }
}
