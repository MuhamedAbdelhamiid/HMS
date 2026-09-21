using System.ComponentModel.DataAnnotations;

namespace HMS.Shared.DTOs.ServiceModuleDTOs
{
    public class NewAssignForStaffDTO
    {
        [Required(ErrorMessage = "Staff id is required.")]
        public string StaffId { get; set; } = default!;
    }
}
