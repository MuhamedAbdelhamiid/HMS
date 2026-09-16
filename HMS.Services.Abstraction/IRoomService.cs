using HMS.Shared.DTOs.RoomModuleDTOs;
using HMS.Shared.QueryParameters.RoomModule;
using HMS.Shared.Responses;
using Microsoft.AspNetCore.Http;

namespace HMS.Services.Abstraction
{
    public interface IRoomService
    {
        Task<GenericResponse<IEnumerable<RoomDTO>>> GetAllRoomsAsync(
            RoomQueryParameters? queryParameters);
        Task<GenericResponse<IEnumerable<AdminRoomDTO>>> GetAllRoomsForAdminAsync(AdminRoomQueryParameters? adminQueryParameters);
        Task<GenericResponse<RoomDetailsDTO>> GetRoomByIdAsync(int id);
        Task<GenericResponse<bool>> CreateRoomAsync(AdminRoomCreationDTO roomToCreate);
        Task<GenericResponse<bool>> UpdateRoomAsync(int id, AdminRoomUpdateDTO roomToUpdate);
        Task<GenericResponse<bool>> DeleteRoomAsync(int id);
        Task<GenericResponse<bool>> UploadRoomImageAsync(int roomId, List<IFormFile> imageFiles);

        Task<GenericResponse<bool>> DeleteRoomImagesAsync(int roomId, int imageId);
    }
}
