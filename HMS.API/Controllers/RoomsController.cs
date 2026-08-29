using HMS.Services.Abstraction;
using HMS.Shared;
using HMS.Shared.DTOs.RoomModuleDTOs;
using HMS.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace HMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
            => _roomService = roomService;

        [HttpGet("public")]
        public async Task<ActionResult<GenericResponse<IEnumerable<RoomDTO>>>> GetAll([FromQuery] RoomQueryParameters queryParameters)
        {
            var result = await _roomService.GetAllRoomsAsync(queryParameters);

            return result;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GenericResponse<RoomDetailsDTO>>> GetById(int id)
        {
            var result = await _roomService.GetRoomByIdAsync(id);

            return result;
        }
    }
}
