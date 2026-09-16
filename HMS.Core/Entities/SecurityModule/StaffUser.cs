using HMS.Core.Entities.Enums.SecurityEnums;

namespace HMS.Core.Entities.SecurityModule
{
    public class StaffUser : HotelUser
    {
        public StaffSpecialities Specialities { get; set; }
    }
}
