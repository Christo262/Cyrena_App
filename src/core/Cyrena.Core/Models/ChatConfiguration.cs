namespace Cyrena.Models
{
    /// <summary>
    /// Configuration that is saved for chats
    /// </summary>
    public sealed class ChatConfiguration : Entity
    {
        public const string Icon = "icon";
        public const string Group = "group";
        public ChatConfiguration()
        {
            Properties = new Dictionary<string, string?>();
        }

        /// <summary>
        /// Safer to interate over <see cref="Properties"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string? this[string key]
        {
            get
            {
                if(!Properties.ContainsKey(key))
                    return null;
                return Properties[key];
            }
            set
            {
                Properties[key] = value; 
            }
        }

        /// <summary>
        /// Friendly title of the chat
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Identifies the <see cref="Cyrena.Contracts.IAssistantMode"/> from DI
        /// </summary>
        public string AssistantModeId { get; set; } = default!;
        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// Last modified date
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Id of the <see cref="Contracts.IConnection"/> provided by <see cref="Contracts.IConnectionProvider"/>
        /// </summary>
        public string ConnectionId { get; set; } = default!;

        /// <summary>
        /// Any additional information
        /// </summary>
        public Dictionary<string, string?> Properties { get; set; }
    }
}
