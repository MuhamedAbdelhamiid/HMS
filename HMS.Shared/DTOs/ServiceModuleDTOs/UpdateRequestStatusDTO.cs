using System.ComponentModel.DataAnnotations;

namespace HMS.Shared.DTOs.ServiceModuleDTOs
{
    public class UpdateRequestStatusDTO
    {
        [Required(ErrorMessage = "Request id is required.")]
        public Guid RequestId { get; set; }
        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = default!;
    }
}
