using HMS.Services.Abstraction;
using HMS.Shared.DTOs.SecurityModuleDTOs;
using HMS.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.API.Controllers
{
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<GenericResponse<UserResponseDTO>>> Login([FromBody] UserLoginDTO userLogin)
        {
            var result = await _authService.LoginAsync(userLogin);

            return HandleResponse(result);
        }

        [HttpPost("register")]
        public async Task<ActionResult<GenericResponse<UserResponseDTO>>> Register([FromBody] UserRegisterDTO userRegister)
        {
            var result = await _authService.RegisterAsync(userRegister);

            return HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create-user")]
        public async Task<ActionResult<GenericResponse<bool>>> CreateStaff([FromBody] StaffCreationDTO staffCreationDTO)
        {
            var result = await _authService.CreateStaffAccountAsync(staffCreationDTO);

            return HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("users/{id}/deactivate")]
        public async Task<ActionResult<GenericResponse<bool>>> Deactivate([FromRoute] string id)
        {
            var result = await _authService.DeactivateUserAsync(id);

            return HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("users/{id}/activate")]
        public async Task<ActionResult<GenericResponse<bool>>> Activate([FromRoute] string id)
        {
            var result = await _authService.ActivateUserAsync(id);

            return HandleResponse(result);
        }

        [Authorize]
        [HttpGet("email-exists")]
        public async Task<ActionResult<GenericResponse<bool>>> CheckEmail([FromQuery] string email)
        {
            var result = await _authService.CheckEmailExistsAsync(email);
            return HandleResponse(result);
        }
    }
}
