using HMS.Shared.DTOs.SecurityModuleDTOs;
using HMS.Shared.Responses;

namespace HMS.Services.Abstraction
{
    public interface IAuthService
    {
        Task<GenericResponse<UserResponseDTO>> LoginAsync(UserLoginDTO userLoginDTO);
        Task<GenericResponse<UserResponseDTO>> RegisterAsync(UserRegisterDTO userRegisterDTO);
        Task<GenericResponse<bool>> CreateStaffAccountAsync(StaffCreationDTO staffCreationDTO);
        Task<GenericResponse<bool>> DeactivateUserAsync(string userId);
        Task<GenericResponse<bool>> ActivateUserAsync(string userId);
        Task<GenericResponse<bool>> CheckEmailExistsAsync(string email);
        Task<GenericResponse<IEnumerable<UserInfoDTO>>> GetAllUsersAsync();
        Task<GenericResponse<UserInfoDTO>> GetUserInfoAsync(string userId);
        Task<GenericResponse<IEnumerable<StaffDTO>>> GetAllStaffAsync();
    }
}
