using HMS.Core.Entities.BookingModule;
using HMS.Core.Entities.Enums.BookingEnums;
using HMS.Core.Entities.Enums.RoomEnums;
using HMS.Core.Entities.RoomModule;
using HMS.Shared.QueryParameters.BookingModule;
using HMS.Shared.QueryParameters.RoomModule;
using System.Linq.Expressions;

namespace HMS.Services.Helpers
{
    public static class FilterHelper
    {
        public static Expression<Func<BookingEntity, bool>> BuildFilterExpression(BookingQueryParams queryParams)
        {
            BookingStatus? parsedStatus = null;
            if (!string.IsNullOrEmpty(queryParams.BookingStatus))
            {
                if (Enum.TryParse<BookingStatus>(queryParams.BookingStatus, true, out var status))
                    parsedStatus = status;
            }

            return b =>
                (!queryParams.CheckInDate.HasValue || b.CheckInDate >= queryParams.CheckInDate.Value) &&
                (!queryParams.CheckOutDate.HasValue || b.CheckOutDate <= queryParams.CheckOutDate.Value) &&
                (parsedStatus == null || b.Status == parsedStatus.Value);
        }

        public static Expression<Func<Room, bool>> BuildFilterExpression(RoomQueryParameters queryParameters)
            => r =>
                (r.Status == RoomStatus.Available || r.Status == RoomStatus.Reserved) &&
                (string.IsNullOrEmpty(queryParameters.RoomType) ||
                    r.RoomType == RoomServiceHelper.GetRoomType(queryParameters.RoomType)) &&
                (!queryParameters.MaxPrice.HasValue || r.PricePerNight <= queryParameters.MaxPrice.Value) &&
                (!queryParameters.MinPrice.HasValue || r.PricePerNight >= queryParameters.MinPrice.Value);

        public static Expression<Func<Room, bool>> BuildFilterExpression(
            AdminRoomQueryParameters queryParameters)
            => r =>
                (string.IsNullOrEmpty(queryParameters.Status) ||
                    r.Status == RoomServiceHelper.GetRoomStatus(queryParameters.Status)) &&
                (string.IsNullOrEmpty(queryParameters.RoomType) ||
                    r.RoomType == RoomServiceHelper.GetRoomType(queryParameters.RoomType)) &&
                (!queryParameters.MaxPrice.HasValue ||
                    r.PricePerNight <= queryParameters.MaxPrice.Value) &&
                (!queryParameters.MinPrice.HasValue ||
                    r.PricePerNight >= queryParameters.MinPrice.Value);
    }
}
