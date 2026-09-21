namespace HMS.Shared.DTOs.ServiceModuleDTOs
{
    public class ServiceRequestDTO
    {
        public string StaffId { get; set; } = default!;
        public string ServiceName { get; set; } = default!;
        public string GuestId { get; set; } = default!;
        public int RoomNumber { get; set; }
        public string? Notes { get; set; }
    }
}
