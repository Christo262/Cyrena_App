using System.ComponentModel.DataAnnotations;

namespace MyBlazorApp.Models
{
    /// <summary>
    /// Represents a contact form submission with validation rules.
    /// </summary>
    public class ContactViewModel
    {
        /// <summary>
        /// The full name of the person submitting the contact form.
        /// </summary>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The email address for reply.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The subject of the message.
        /// </summary>
        [Required(ErrorMessage = "Subject is required.")]
        [StringLength(150, ErrorMessage = "Subject cannot exceed 150 characters.")]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// The body of the message.
        /// </summary>
        [Required(ErrorMessage = "Message is required.")]
        [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters.")]
        public string Message { get; set; } = string.Empty;
    }
}
