using System;

namespace MyLib.Options
{
    /// <summary>
    /// Configuration options for the email service.
    /// </summary>
    public class EmailOptions
    {
        /// <summary>
        /// The SMTP host.
        /// </summary>
        public string SmtpHost { get; set; } = string.Empty;

        /// <summary>
        /// The SMTP port.
        /// </summary>
        public int SmtpPort { get; set; } = 25;

        /// <summary>
        /// Whether to use SSL.
        /// </summary>
        public bool UseSsl { get; set; } = false;

        /// <summary>
        /// The sender email address.
        /// </summary>
        public string Sender { get; set; } = string.Empty;

        /// <summary>
        /// Username for SMTP authentication.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Password for SMTP authentication.
        /// </summary>
        public string? Password { get; set; }
    }
}