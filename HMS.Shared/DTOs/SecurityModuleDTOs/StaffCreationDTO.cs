using System.ComponentModel.DataAnnotations;

namespace HMS.Shared.DTOs.SecurityModuleDTOs
{
    public class StaffCreationDTO
    {
        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; } = default!;
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = default!;
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = default!;
        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string PhoneNumber { get; set; } = default!;
        [Required(ErrorMessage = "Specialty is required.")]
        public string Specialty { get; set; } = default!;
    }
}
