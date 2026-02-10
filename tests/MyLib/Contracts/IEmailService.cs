using System.Threading.Tasks;

namespace MyLib.Contracts
{
    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
