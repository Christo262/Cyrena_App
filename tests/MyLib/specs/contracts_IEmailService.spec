{
  "Id": "contracts_IEmailService",
  "Title": "IEmailService Contract Specification",
  "Summary": "Specification for IEmailService contract",
  "Keywords": [
    "email",
    "service",
    "contract"
  ],
  "Content": "The IEmailService contract defines a single asynchronous method for sending emails.\n\nMethod:\n- SendEmailAsync(string to, string subject, string body) : Task\n\nParameters:\n- to: Recipient email address.\n- subject: Email subject.\n- body: Email body.\n\nReturn:\n- Task representing the asynchronous operation.\n\nThis contract is intended for dependency injection and is implemented by EmailService.\n",
  "Link": null,
  "FilePath": null
}