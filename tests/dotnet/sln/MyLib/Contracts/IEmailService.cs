using System.Threading.Tasks;

namespace MyLib.Contracts
{
    /// <summary>
    /// Contract for sending emails.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="to">Recipient email address.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="body">Email body.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendAsync(string to, string subject, string body);
    }
}