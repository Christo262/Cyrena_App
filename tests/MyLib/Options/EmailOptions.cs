using System;

namespace ExampleNamespace.Options
{
    public class EmailOptions
    {
        public string? SmtpHost { get; set; }
        public int SmtpPort { get; set; } = 25;
        public bool EnableSsl { get; set; }
        public string? SmtpUser { get; set; }
        public string? SmtpPassword { get; set; }
        public string? SenderEmail { get; set; }
        public string? SenderName { get; set; }
        public bool IsBodyHtml { get; set; } = false;
    }
}
