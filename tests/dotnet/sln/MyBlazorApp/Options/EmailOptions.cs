namespace BlazorApp.Options
{
    public class EmailOptions
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public string FromAddress { get; set; } = string.Empty;
        public string FromDisplayName { get; set; } = string.Empty;
        public string SubjectPrefix { get; set; } = "[Bug Report]";
        public List<EmailRecipient> Recipients { get; set; } = new();
    }

    public class EmailRecipient
    {
        public string Address { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}
