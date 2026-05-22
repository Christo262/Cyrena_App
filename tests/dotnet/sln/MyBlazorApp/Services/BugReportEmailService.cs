using System.Net;
using System.Net.Mail;
using BlazorApp.Components.Pages;
using BlazorApp.Contracts;
using BlazorApp.Options;
using Microsoft.Extensions.Options;

namespace BlazorApp.Services
{
    public class BugReportEmailService : IBugReportEmailService
    {
        private readonly EmailOptions _options;
        private readonly ILogger<BugReportEmailService> _logger;

        public BugReportEmailService(IOptions<EmailOptions> options, ILogger<BugReportEmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendBugReportAsync(BugReport.BugReportModel bugReport)
        {
            try
            {
                using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
                {
                    EnableSsl = _options.EnableSsl,
                    Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword)
                };

                var from = new MailAddress(_options.FromAddress, _options.FromDisplayName);

                using var message = new MailMessage
                {
                    From = from,
                    Subject = $"{_options.SubjectPrefix} {bugReport.Title}",
                    Body = BuildEmailBody(bugReport),
                    IsBodyHtml = true
                };

                // Add all recipients
                foreach (var recipient in _options.Recipients)
                {
                    if (!string.IsNullOrWhiteSpace(recipient.Address))
                    {
                        message.To.Add(new MailAddress(recipient.Address, recipient.DisplayName));
                    }
                }

                // Fallback: if no recipients configured, log warning but don't throw
                if (message.To.Count == 0)
                {
                    _logger.LogWarning("No email recipients configured. Bug report for '{Title}' was not sent.", bugReport.Title);
                    return;
                }

                await client.SendMailAsync(message);
                _logger.LogInformation("Bug report email sent successfully to {RecipientCount} recipient(s) for: {Title}", message.To.Count, bugReport.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send bug report email for: {Title}", bugReport.Title);
                throw;
            }
        }

        private static string BuildEmailBody(BugReport.BugReportModel bugReport)
        {
            var severityColor = bugReport.Severity?.ToLower() switch
            {
                "critical" => "#ef4444",
                "high" => "#f97316",
                "medium" => "#f59e0b",
                "low" => "#10b981",
                _ => "#6366f1"
            };

            var severityBg = bugReport.Severity?.ToLower() switch
            {
                "critical" => "#fee2e2",
                "high" => "#ffedd5",
                "medium" => "#fef3c7",
                "low" => "#d1fae5",
                _ => "#e0e7ff"
            };

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Bug Report - {System.Net.WebUtility.HtmlEncode(bugReport.Title)}</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f8fafc; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background-color: #f8fafc; padding: 40px 20px;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; width: 100%; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1);'>
                    <!-- Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #6366f1, #8b5cf6); padding: 40px 32px; text-align: center;'>
                            <table cellpadding='0' cellspacing='0' border='0' align='center'>
                                <tr>
                                    <td style='padding-bottom: 16px;'>
                                        <svg width='48' height='48' viewBox='0 0 16 16' fill='white' xmlns='http://www.w3.org/2000/svg'>
                                            <path d='M4.355.522a.5.5 0 0 1 .623.333l.291.956A4.979 4.979 0 0 1 8 3c1.669 0 3.218.51 4.5 1.385l.291-.956a.5.5 0 1 1 .958.291l-.41 1.352a5.001 5.001 0 0 1 2.25 3.42l1.552.287a.5.5 0 0 1 .179.925 5.02 5.02 0 0 1-.513 1.1l1.552.287a.5.5 0 0 1 .179.925 5.02 5.02 0 0 1-1.513 1.552l.287 1.552a.5.5 0 0 1-.925.179 5.02 5.02 0 0 1-1.1-.513l-.287 1.552a.5.5 0 0 1-.925-.179 5.02 5.02 0 0 1 .513-1.1l-.287-1.552a.5.5 0 0 1 .179-.925 5.02 5.02 0 0 1 1.552-1.513l-.287-1.552a.5.5 0 0 1 .179-.925 5.02 5.02 0 0 1 1.1.513l.287-1.552a.5.5 0 0 1 .925.179zM8 4a4 4 0 1 0 0 8 4 4 0 0 0 0-8z'/>
                                        </svg>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <h1 style='margin: 0; color: #ffffff; font-size: 24px; font-weight: 700; letter-spacing: -0.025em;'>Bug Report Received</h1>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style='padding: 32px;'>
                            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                                <!-- Severity Badge -->
                                <tr>
                                    <td style='padding-bottom: 24px;'>
                                        <span style='display: inline-block; padding: 6px 16px; border-radius: 9999px; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.05em; background-color: {severityBg}; color: {severityColor};'>
                                            {System.Net.WebUtility.HtmlEncode(bugReport.Severity ?? "Unknown")}
                                        </span>
                                    </td>
                                </tr>
                                
                                <!-- Title -->
                                <tr>
                                    <td style='padding-bottom: 8px;'>
                                        <p style='margin: 0; font-size: 12px; font-weight: 600; color: #94a3b8; text-transform: uppercase; letter-spacing: 0.05em;'>Title</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding-bottom: 24px;'>
                                        <p style='margin: 0; font-size: 18px; font-weight: 700; color: #1e293b; line-height: 1.4;'>
                                            {System.Net.WebUtility.HtmlEncode(bugReport.Title)}
                                        </p>
                                    </td>
                                </tr>
                                
                                <!-- Description -->
                                <tr>
                                    <td style='padding-bottom: 8px;'>
                                        <p style='margin: 0; font-size: 12px; font-weight: 600; color: #94a3b8; text-transform: uppercase; letter-spacing: 0.05em;'>Description</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding-bottom: 24px;'>
                                        <div style='background-color: #f8fafc; border-radius: 10px; padding: 20px; border: 1px solid #e2e8f0;'>
                                            <p style='margin: 0; font-size: 14px; color: #475569; line-height: 1.7; white-space: pre-wrap;'>
                                                {System.Net.WebUtility.HtmlEncode(bugReport.Description)}
                                            </p>
                                        </div>
                                    </td>
                                </tr>
                                
                                <!-- Metadata -->
                                <tr>
                                    <td style='border-top: 1px solid #e2e8f0; padding-top: 24px;'>
                                        <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                                            <tr>
                                                <td width='50%' style='padding-right: 8px;'>
                                                    <p style='margin: 0 0 4px 0; font-size: 11px; font-weight: 600; color: #94a3b8; text-transform: uppercase; letter-spacing: 0.05em;'>Reported At</p>
                                                    <p style='margin: 0; font-size: 14px; font-weight: 600; color: #1e293b;'>{DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                                                </td>
                                                <td width='50%' style='padding-left: 8px;'>
                                                    <p style='margin: 0 0 4px 0; font-size: 11px; font-weight: 600; color: #94a3b8; text-transform: uppercase; letter-spacing: 0.05em;'>Severity Level</p>
                                                    <p style='margin: 0; font-size: 14px; font-weight: 600; color: {severityColor};'>{System.Net.WebUtility.HtmlEncode(bugReport.Severity ?? "Unknown")}</p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8fafc; padding: 24px 32px; text-align: center; border-top: 1px solid #e2e8f0;'>
                            <p style='margin: 0; font-size: 12px; color: #94a3b8;'>
                                This bug report was submitted via <strong style='color: #6366f1;'>MyBlazorApp</strong>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }
    }
}
