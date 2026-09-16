using HMS.Services.Abstraction;
using HMS.Shared.DTOs.RoomModuleDTOs;
using HMS.Shared.QueryParameters.RoomModule;
using HMS.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.API.Controllers
{
    public class RoomsController : BaseApiController
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
            => _roomService = roomService;

        #region Guest Endpoints
        [HttpGet("public")]
        public async Task<ActionResult<GenericResponse<IEnumerable<RoomDTO>>>> GetAll([FromQuery] RoomQueryParameters? queryParameters)
        {
            var result = await _roomService.GetAllRoomsAsync(queryParameters);

            return HandleResponse(result);
        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<GenericResponse<RoomDetailsDTO>>> GetById(int id)
        {
            var result = await _roomService.GetRoomByIdAsync(id);

            return HandleResponse(result);

        }
        #endregion

        #region Admin Endpoints
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("admin")]
        public async Task<ActionResult<GenericResponse<IEnumerable<AdminRoomDTO>>>> GetAll([FromQuery] AdminRoomQueryParameters? queryParameters)
        {
            var result = await _roomService.GetAllRoomsForAdminAsync(queryParameters);

            return HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<GenericResponse<bool>>> CreateRoom([FromBody] AdminRoomCreationDTO roomToCreate)
        {
            var result = await _roomService.CreateRoomAsync(roomToCreate);

            return HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<GenericResponse<bool>>> UpdateRoom(int id, [FromBody] AdminRoomUpdateDTO roomToUpdate)
        {
            var result = await _roomService.UpdateRoomAsync(id, roomToUpdate);

            return HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<GenericResponse<bool>>> DeleteRoom(int id)
        {
            var result = await _roomService.DeleteRoomAsync(id);

            return HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/images")]
        public async Task<ActionResult<GenericResponse<bool>>> UploadImages([FromRoute] int id, [FromForm] List<IFormFile> files)
        {
            var result = await _roomService.UploadRoomImageAsync(id, files);

            return HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}/images/{imageId}")]
        public async Task<ActionResult<GenericResponse<bool>>> DeleteImages([FromRoute] int id, [FromRoute] int imageId)
        {
            var result = await _roomService.DeleteRoomImagesAsync(id, imageId);
            return HandleResponse(result);
        }
        #endregion
    }
}
