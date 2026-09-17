using HMS.Core.Entities.Enums.RoomEnums;
using HMS.Core.Entities.RoomModule;
using System.Linq.Expressions;

namespace HMS.Services.Helpers
{
    // This class provides some helper methods across all project to use it in room service implementation and also the profile of room profile
    public static class RoomServiceHelper
    {
        public static void BuildSortExpression(
            string? sort,
            out Expression<Func<Room, object>>? orderBy,
            out Expression<Func<Room, object>>? orderByDesc)
        {
            orderBy = null;
            orderByDesc = null;

            switch (sort)
            {
                case "priceAsc":
                    orderBy = r => r.PricePerNight;
                    break;
                case "priceDesc":
                    orderByDesc = r => r.PricePerNight;
                    break;
                default:
                    orderBy = r => r.Id;
                    break;
            }
        }

        public static RoomType? GetRoomType(string roomType)
         => roomType.ToLower() switch
         {
             "single" => RoomType.Single,
             "double" => RoomType.Double,
             "triple" => RoomType.Triple,
             _ => null
         };

        public static RoomStatus? GetRoomStatus(string roomStatus)
            => roomStatus.ToLower() switch
            {
                "available" => RoomStatus.Available,
                "reserved" => RoomStatus.Reserved,
                "maintenance" => RoomStatus.InMaintenance,
                "notexist" => RoomStatus.NotExist,
                _ => null
            };
    }
}
