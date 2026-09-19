using HMS.Core.Entities.Enums.ServiceModule;
using HMS.Core.Entities.SecurityModule;

namespace HMS.Core.Entities.ServiceModule
{
    public class ServiceRequest : BaseEntity<Guid>
    {
        public int ServiceId { get; set; }
        public Service Service { get; set; } = default!;

        public string UserId { get; set; } = default!;
        public HotelUser User { get; set; } = default!;


        public string? StaffId { get; set; }
        public StaffUser? Staff { get; set; }

        public string? AdminId { get; set; }
        public HotelUser? Admin { get; set; }

        public int RoomNumber { get; set; }

        public ServiceRequestStatus Status { get; set; } = default!;
        public string? Notes { get; set; }
    }
}
