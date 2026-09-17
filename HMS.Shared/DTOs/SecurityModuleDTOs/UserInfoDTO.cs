namespace HMS.Shared.DTOs.SecurityModuleDTOs
{
    public class UserInfoDTO
    {
        public string Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public bool IsActive { get; set; }
        public string UserName { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public ICollection<UserBookingDTO> Bookings { get; set; } = default!;
    }
}
