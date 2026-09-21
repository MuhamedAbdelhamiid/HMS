using HMS.Services.Abstraction;
using HMS.Shared.DTOs.SecurityModuleDTOs;
using HMS.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<GenericResponse<UserResponseDTO>>> Login([FromBody] UserLoginDTO userLogin)
        {
            var result = await _authService.LoginAsync(userLogin);

            return HandleResponse(result);
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<GenericResponse<UserResponseDTO>>> Register([FromBody] UserRegisterDTO userRegister)
        {
            var result = await _authService.RegisterAsync(userRegister);

            return HandleResponse(result);
        }

        [HttpPost("create-staff")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<GenericResponse<bool>>> CreateStaff([FromBody] StaffCreationDTO staffCreationDTO)
        {
            var result = await _authService.CreateStaffAccountAsync(staffCreationDTO);

            return HandleResponse(result);
        }

        [HttpPut("users/{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GenericResponse<bool>>> Deactivate([FromRoute] string id)
        {
            var result = await _authService.DeactivateUserAsync(id);

            return HandleResponse(result);
        }

        [HttpPut("users/{id}/activate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GenericResponse<bool>>> Activate([FromRoute] string id)
        {
            var result = await _authService.ActivateUserAsync(id);

            return HandleResponse(result);
        }

        [HttpGet("email-exists")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GenericResponse<bool>>> CheckEmail([FromQuery] string email)
        {
            var result = await _authService.CheckEmailExistsAsync(email);
            return HandleResponse(result);
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GenericResponse<IEnumerable<UserInfoDTO>>>> GetAllUsers()
        {
            var result = await _authService.GetAllUsersAsync();
            return HandleResponse(result);
        }

        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GenericResponse<IEnumerable<UserInfoDTO>>>> GetUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _authService.GetUserInfoAsync(userId!);
            return HandleResponse(result);
        }

        [HttpGet("staff")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GenericResponse<IEnumerable<StaffDTO>>>> GetAllStaff()
        {
            var result = await _authService.GetAllStaffAsync();
            return HandleResponse(result);
        }
    }
}
