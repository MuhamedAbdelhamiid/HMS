using AutoMapper;
using HMS.Core.Contracts;
using HMS.Core.Entities.BookingModule;
using HMS.Core.Entities.Enums.BookingEnums;
using HMS.Core.Entities.Enums.RoomEnums;
using HMS.Core.Entities.RoomModule;
using HMS.Services.Abstraction;
using HMS.Services.Helpers;
using HMS.Shared.DTOs.BookingModuleDTOs;
using HMS.Shared.QueryParameters.BookingModule;
using HMS.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HMS.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<BookingService> _logger;

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<BookingService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<GenericResponse<bool>> CancelBookingAsync(Guid id)
        {
            var bookingRepo = _unitOfWork.GetRepository<BookingEntity, Guid>();

            try
            {
                var booking = await bookingRepo.GetByIdAsync(id);

                if (booking is null)
                    return GenericResponse<bool>.Error("Booking not found.", StatusCodes.Status404NotFound);

                if (booking.CheckOutDate < DateTime.UtcNow)
                    return GenericResponse<bool>.Error("Cannot cancel that booking due to its checkout.", StatusCodes.Status400BadRequest);

                booking.Status = BookingStatus.Cancelled;
                booking.UpdatedAt = DateTime.UtcNow;

                bookingRepo.Update(booking);
                var saved = await _unitOfWork.SaveChangesAsync() > 0;

                if (!saved)
                    return GenericResponse<bool>.Error("Failed to cancel the booking.", StatusCodes.Status500InternalServerError);

                return GenericResponse<bool>.Success(true, "Booking cancelled successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while cancelling booking with id: {BookingId}", id);
                return GenericResponse<bool>.Failure("An error occurred while cancelling the booking.");
            }
        }

        public async Task<GenericResponse<Guid>> CreateBookingAsync(string userId, CreateBookingDTO bookingRequest)
        {
            if (bookingRequest is null)
                return GenericResponse<Guid>.Error(
                    "Invalid booking request.");

            if (bookingRequest.CheckInDate < DateTime.UtcNow.Date || bookingRequest.CheckOutDate < DateTime.UtcNow.Date)
                return GenericResponse<Guid>.Error(
                    "Dates cannot be in the past.");

            var roomRepo = _unitOfWork.GetRepository<Room, int>();
            var room = await roomRepo.GetByIdAsync(bookingRequest.RoomId, r => r.Bookings);

            if (room is null)
                return GenericResponse<Guid>.Error(
                    "Room not found", StatusCodes.Status404NotFound);

            if (!RoomIsAvailableForBookingWithinDate(room, bookingRequest.CheckInDate, bookingRequest.CheckOutDate))
                return GenericResponse<Guid>.Error("Room is not available for the specified dates.", StatusCodes.Status400BadRequest);

            var booking = _mapper.Map<BookingEntity>(bookingRequest);
            booking.HotelUserId = userId;
            booking.TotalAmount = CalculateTotalAmount(room, bookingRequest.CheckInDate, bookingRequest.CheckOutDate);

            try
            {
                var bookingRepo = _unitOfWork.GetRepository<BookingEntity, Guid>();
                await bookingRepo.AddAsync(booking);
                await _unitOfWork.SaveChangesAsync();

                return GenericResponse<Guid>.Success(booking.Id, "Booking created successfully", StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the booking");
                return GenericResponse<Guid>.Failure("An error occurred while creating the booking.");
            }
        }

        public async Task<GenericResponse<IEnumerable<BookingDTO>>> GetAllBookingsForAdminAsync(BookingQueryParams? queryParams)
        {
            var bookingRepo = _unitOfWork.GetRepository<BookingEntity, Guid>();

            IEnumerable<BookingEntity> bookings;

            if (queryParams is not null)
            {
                var filter = FilterHelper.BuildFilterExpression(queryParams);

                bookings = await bookingRepo.GetAllAsync(filter: filter, [b => b.User]);
            }
            else
                bookings = await bookingRepo.GetAllAsync();

            if (bookings is null || !bookings.Any())
                return GenericResponse<IEnumerable<BookingDTO>>.Error("No bookings found.", StatusCodes.Status404NotFound);

            var bookingsToReturn = _mapper.Map<IEnumerable<BookingDTO>>(bookings);

            return GenericResponse<IEnumerable<BookingDTO>>.Success(bookingsToReturn, "Bookings retrieved successfully.");
        }


        #region Helper Methods
        private decimal CalculateTotalAmount(Room room, DateTime checkInDate, DateTime checkOutDate)
        {
            var numberOfDays = (checkOutDate - checkInDate).Days == 0 ? 1 : (checkOutDate - checkInDate).Days;

            var totalAmount = room.PricePerNight * numberOfDays;
            return totalAmount;
        }
        private bool RoomIsAvailableForBookingWithinDate(Room room, DateTime checkInDate, DateTime checkOutDate)
        {
            var bookings = room.Bookings;

            foreach (var booking in bookings)
                if (checkInDate < booking.CheckOutDate && checkOutDate > booking.CheckInDate &&
                    (booking.Status == BookingStatus.Paid || booking.Status == BookingStatus.Pending))
                    return false;

            if (room.Status == RoomStatus.NotExist || room.Status == RoomStatus.InMaintenance)
                return false;

            return true;
        }
        #endregion
    }
}
