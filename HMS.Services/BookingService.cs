using AutoMapper;
using HMS.Core.Contracts;
using HMS.Core.Entities.BookingModule;
using HMS.Core.Entities.Enums.BookingEnums;
using HMS.Core.Entities.Enums.RoomEnums;
using HMS.Core.Entities.RoomModule;
using HMS.Services.Abstraction;
using HMS.Shared.DTOs.BookingModuleDTOs;
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

        public async Task<GenericResponse<Guid>> CreateBookingAsync(string userId, CreateBookingDTO bookingRequest)
        {
            if (bookingRequest is null)
                return GenericResponse<Guid>.Error(
                    "Invalid booking request.", StatusCodes.Status400BadRequest);

            if (bookingRequest.CheckInDate < DateTime.UtcNow.Date || bookingRequest.CheckOutDate < DateTime.UtcNow.Date)
                return GenericResponse<Guid>.Error(
                    "Dates cannot be in the past.", StatusCodes.Status400BadRequest);

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
