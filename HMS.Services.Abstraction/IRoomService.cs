using HMS.Shared;
using HMS.Shared.DTOs.RoomModuleDTOs;
using HMS.Shared.Responses;

namespace HMS.Services.Abstraction
{
    public interface IRoomService
    {
        Task<GenericResponse<IEnumerable<RoomDTO>>> GetAllRoomsAsync(
            RoomQueryParameters queryParameters);

        Task<GenericResponse<RoomDetailsDTO>> GetRoomByIdAsync(int id);
    }
}
