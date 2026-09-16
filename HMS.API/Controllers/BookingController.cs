using HMS.Services.Abstraction;
using HMS.Shared.DTOs.BookingModuleDTOs;
using HMS.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HMS.API.Controllers
{
    public class BookingController : BaseApiController
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<GenericResponse<string>>> CreateBooking([FromBody] CreateBookingDTO bookingRequest)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _bookingService.CreateBookingAsync(userId!, bookingRequest);

            return HandleResponse(result);
        }
    }
}
