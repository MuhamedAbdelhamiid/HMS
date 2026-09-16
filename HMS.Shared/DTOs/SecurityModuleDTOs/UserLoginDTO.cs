using System.ComponentModel.DataAnnotations;

namespace HMS.Shared.DTOs.SecurityModuleDTOs
{
    public class UserLoginDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = default!;
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = default!;
    }
}
