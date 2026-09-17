using HMS.Shared.DTOs.BookingModuleDTOs;
using HMS.Shared.Responses;

namespace HMS.Services.Abstraction
{
    public interface IBookingService
    {
        Task<GenericResponse<Guid>> CreateBookingAsync(string userId, CreateBookingDTO bookingRequest);
    }
}
