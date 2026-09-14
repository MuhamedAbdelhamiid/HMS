using HMS.Core.Entities.Enums.SecurityEnums;

namespace HMS.Services.Helpers
{
    public static class AuthServiceHelper
    {
        public static StaffSpecialities? GetStaffSpeciality(string? speciality)
        {
            if (string.IsNullOrWhiteSpace(speciality))
                return null;

            if (Enum.TryParse<StaffSpecialities>(speciality.Trim(), ignoreCase: true, out var parsed)
                && Enum.IsDefined(typeof(StaffSpecialities), parsed))
                return parsed;

            return speciality.Trim().ToLower() switch
            {
                "housekeeping" => StaffSpecialities.HouseKeeping,
                "food and beverage" => StaffSpecialities.FoodAndBeverage,
                "laundry" => StaffSpecialities.Laundry,
                _ => null
            };
        }
    }
}
