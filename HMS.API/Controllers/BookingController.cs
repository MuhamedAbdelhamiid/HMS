using HMS.Services.Abstraction;
using HMS.Shared.DTOs.BookingModuleDTOs;
using HMS.Shared.QueryParameters.BookingModule;
using HMS.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HMS.API.Controllers
{
    public class BookingController : BaseApiController
    {
        private readonly IBookingService _bookingService;
        private readonly IPaymentService _paymentService;

        public BookingController(IBookingService bookingService, IPaymentService paymentService)
        {
            _bookingService = bookingService;
            _paymentService = paymentService;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<GenericResponse<Guid>>> CreateBooking([FromBody] CreateBookingDTO bookingRequest)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _bookingService.CreateBookingAsync(userId!, bookingRequest);

            return HandleResponse(result);
        }

        [Authorize]
        [HttpPost("{id}/pay")]
        public async Task<ActionResult<GenericResponse<string>>> CreatePaymentUrl(Guid id)
        {
            var result = await _paymentService.ProcessPaymentAsync(id);

            return HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<ActionResult<GenericResponse<IEnumerable<BookingDTO>>>> GetAllBookings([FromQuery] BookingQueryParams? queryParams)
        {
            var result = await _bookingService.GetAllBookingsForAdminAsync(queryParams);

            return HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<GenericResponse<bool>>> CancelBooking(Guid id)
        {
            var result = await _bookingService.CancelBookingAsync(id);

            return HandleResponse(result);
        }
    }
}
