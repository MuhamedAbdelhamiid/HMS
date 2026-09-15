using AutoMapper;
using HMS.Core.Entities.SecurityModule;
using HMS.Services.Abstraction;
using HMS.Services.Helpers;
using HMS.Shared.DTOs.SecurityModuleDTOs;
using HMS.Shared.Messages;
using HMS.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HMS.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<HotelUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<HotelUser> userManager,
            IConfiguration configuration,
            IMapper mapper,
            IEmailService emailService,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _mapper = mapper;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<GenericResponse<UserResponseDTO>> LoginAsync(UserLoginDTO userLoginDTO)
        {
            var genericResponse = new GenericResponse<UserResponseDTO>()
            {
                StatusCode = StatusCodes.Status200OK
            };

            try
            {
                if (userLoginDTO is null)
                {
                    genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                    genericResponse.Message = "No data was sent.";
                    return genericResponse;
                }

                var user = await _userManager.FindByEmailAsync(userLoginDTO.Email);

                if (user is null)
                {
                    genericResponse.StatusCode = StatusCodes.Status401Unauthorized;
                    genericResponse.Message = "Invalid Credentials.";
                    return genericResponse;
                }

                if (!user.IsActive)
                {
                    genericResponse.StatusCode = StatusCodes.Status423Locked;
                    genericResponse.Message = "User account are locked, Please contact with administration.";
                    return genericResponse;
                }


                var userIsAuthenticated = await _userManager.CheckPasswordAsync(user, userLoginDTO.Password);

                if (!userIsAuthenticated)
                {
                    genericResponse.StatusCode = StatusCodes.Status401Unauthorized;
                    genericResponse.Message = "Invalid Credentials.";
                    return genericResponse;
                }

                var token = await GenerateTokenAsync(user);
                genericResponse.Data = new UserResponseDTO(token, user.UserName!, user.Email!);
                genericResponse.Message = "User logged in successfully.";

                return genericResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging in.");
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Message = "An error occurred while logging in.";
                return genericResponse;
            }
        }

        public async Task<GenericResponse<UserResponseDTO>> RegisterAsync(UserRegisterDTO userRegisterDTO)
        {
            var genericResponse = new GenericResponse<UserResponseDTO>()
            {
                StatusCode = StatusCodes.Status200OK
            };

            try
            {
                if (userRegisterDTO is null)
                {
                    genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                    genericResponse.Message = "Invalid user data";
                    return genericResponse;
                }

                var userWithThisEmail = await _userManager.FindByEmailAsync(userRegisterDTO.Email);

                if (userWithThisEmail is not null)
                {
                    genericResponse.StatusCode = StatusCodes.Status409Conflict;
                    genericResponse.Message = "Email already exists, please add a new one";
                    return genericResponse;
                }

                var userToAdd = _mapper.Map<HotelUser>(userRegisterDTO);
                var result = await _userManager.CreateAsync(userToAdd, userRegisterDTO.Password);

                if (!result.Succeeded)
                {
                    genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                    genericResponse.Message = string.Join(", ", result.Errors.Select(e => e.Description));
                    return genericResponse;
                }

                await _userManager.AddToRoleAsync(userToAdd, "Guest");

                var token = await GenerateTokenAsync(userToAdd);

                genericResponse.Data = new UserResponseDTO(token, userToAdd.UserName!, userToAdd.Email!);
                genericResponse.Message = "Account created successfully.";

                try
                {
                    var emailToSend = new Email()
                    {
                        To = userToAdd.Email!,
                        Subject = "Welcome Message",
                        Message = $"Welcome To Our Hotel, {userToAdd.FullName}"
                    };

                    await _emailService.SendEmailAsync(emailToSend);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send welcome email.");
                }

                return genericResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering the user.");
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Message = "An error occurred while creating the account.";
                return genericResponse;
            }
        }

        public async Task<GenericResponse<bool>> CreateStaffAccountAsync(StaffCreationDTO staffCreationDTO)
        {
            var genericResponse = new GenericResponse<bool>()
            {
                StatusCode = StatusCodes.Status200OK
            };

            try
            {
                if (staffCreationDTO is null)
                {
                    genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                    genericResponse.Message = "Invalid staff data";
                    return genericResponse;
                }

                var userWithThisEmail = await _userManager.FindByEmailAsync(staffCreationDTO.Email);

                if (userWithThisEmail is not null)
                {
                    genericResponse.StatusCode = StatusCodes.Status409Conflict;
                    genericResponse.Message = "Email already exists, please add a new one";
                    return genericResponse;
                }

                var staffSpeciality = AuthServiceHelper.GetStaffSpeciality(staffCreationDTO.Specialty);

                if (staffSpeciality is null)
                {
                    genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                    genericResponse.Message = "Invalid staff specialty.";
                    return genericResponse;
                }

                var staffToAdd = _mapper.Map<StaffUser>(staffCreationDTO);
                staffToAdd.UserName = staffCreationDTO.Email;
                staffToAdd.Specialities = staffSpeciality.Value;

                var createResult = await _userManager.CreateAsync(staffToAdd, staffCreationDTO.Password);

                if (!createResult.Succeeded)
                {
                    genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                    genericResponse.Message = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return genericResponse;
                }

                var roleResult = await _userManager.AddToRoleAsync(staffToAdd, "Staff");

                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(staffToAdd);
                    genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                    genericResponse.Message = "Failed to assign Staff role. The user was not created.";
                    return genericResponse;
                }

                genericResponse.Data = true;
                genericResponse.Message = "Staff account created successfully.";
                return genericResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a staff account.");
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Message = "An error occurred while creating the staff account.";
                genericResponse.Data = false;
                return genericResponse;
            }
        }

        public async Task<GenericResponse<bool>> DeactivateUserAsync(string userId)
        => await SetUserActiveStateAsync(userId, isActive: false);

        public async Task<GenericResponse<bool>> ActivateUserAsync(string userId)
        => await SetUserActiveStateAsync(userId, isActive: true);

        public async Task<GenericResponse<bool>> CheckEmailExistsAsync(string email)
        {
            var genericResponse = new GenericResponse<bool>()
            {
                StatusCode = StatusCodes.Status200OK
            };

            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                    genericResponse.Message = "Email is required.";
                    genericResponse.Data = false;
                    return genericResponse;
                }

                var user = await _userManager.FindByEmailAsync(email);
                genericResponse.Data = user is not null;
                genericResponse.Message = "Email check completed.";
                return genericResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking email existence.");
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Message = "An error occurred while checking the email.";
                genericResponse.Data = false;
                return genericResponse;
            }
        }

        #region Helper Methods
        private async Task<GenericResponse<bool>> SetUserActiveStateAsync(string userId, bool isActive)
        {
            var genericResponse = new GenericResponse<bool>()
            {
                StatusCode = StatusCodes.Status200OK
            };

            var actionName = isActive ? "activating" : "deactivating";

            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                    genericResponse.Message = "User id is required.";
                    genericResponse.Data = false;
                    return genericResponse;
                }

                var user = await _userManager.FindByIdAsync(userId);

                if (user is null)
                {
                    genericResponse.StatusCode = StatusCodes.Status404NotFound;
                    genericResponse.Message = "User was not found.";
                    genericResponse.Data = false;
                    return genericResponse;
                }

                user.IsActive = isActive;
                user.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                    genericResponse.Message = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    genericResponse.Data = false;
                    return genericResponse;
                }

                genericResponse.Data = true;
                genericResponse.Message = isActive
                    ? "User activated successfully."
                    : "User deactivated successfully.";
                return genericResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while {Action} user with id {UserId}.", actionName, userId);
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Message = $"An error occurred while {actionName} the user.";
                genericResponse.Data = false;
                return genericResponse;
            }
        }

        public async Task<string> GenerateTokenAsync(HotelUser user)
        {
            var userClaims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.NameId, user.Id!),
                new Claim("Activity", user.IsActive.ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
                userClaims.Add(new Claim(ClaimTypes.Role, role));

            var secretKey = _configuration.GetSection("JWTOptions")["SecretKey"];
            var issuer = _configuration.GetSection("JWTOptions")["Issuer"];
            var audience = _configuration.GetSection("JWTOptions")["Audience"];

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey!)
                );

            var singingCred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: userClaims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: singingCred
                );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
        #endregion
    }
}
