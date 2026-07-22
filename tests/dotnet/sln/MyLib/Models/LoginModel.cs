using System.ComponentModel.DataAnnotations;

namespace MyLib.Models
{
    /// <summary>
    /// Represents a reusable login model with built-in data validation.
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// The username or email address used to log in.
        /// </summary>
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(256, ErrorMessage = "Username cannot exceed 256 characters.")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The password for authentication.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(128, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 128 characters.")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the user wants to persist their login session across browser sessions.
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
