using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyLib.Contracts;
using MyLib.Options;

namespace MyLib.Services
{
    /// <summary>
    /// Default implementation of <see cref="IEmailService"/>.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _options;

        public EmailService(IOptions<EmailOptions> options)
        {
            _options = options.Value;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
            {
                EnableSsl = _options.UseSsl,
                Credentials = string.IsNullOrEmpty(_options.Username)
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(_options.Username, _options.Password)
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_options.Sender),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            mail.To.Add(to);

            await client.SendMailAsync(mail).ConfigureAwait(false);
        }
    }
}