using HMS.Shared.DTOs.BookingModuleDTOs;
using HMS.Shared.QueryParameters.BookingModule;
using HMS.Shared.Responses;

namespace HMS.Services.Abstraction
{
    public interface IBookingService
    {
        Task<GenericResponse<Guid>> CreateBookingAsync(string userId, CreateBookingDTO bookingRequest);
        Task<GenericResponse<IEnumerable<BookingDTO>>> GetAllBookingsForAdminAsync(BookingQueryParams? queryParams);
        Task<GenericResponse<bool>> CancelBookingAsync(Guid id);
    }
}
