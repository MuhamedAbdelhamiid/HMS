using AutoMapper;
using HMS.Core.Entities.SecurityModule;
using HMS.Services.Abstraction;
using HMS.Services.Helpers;
using HMS.Shared.DTOs.SecurityModuleDTOs;
using HMS.Shared.Messages;
using HMS.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            try
            {
                if (userLoginDTO is null)
                    return GenericResponse<UserResponseDTO>.Error("No data was sent.", StatusCodes.Status400BadRequest);

                var user = await _userManager.FindByEmailAsync(userLoginDTO.Email);

                if (user is null)
                    return GenericResponse<UserResponseDTO>.Error("Invalid Credentials.", StatusCodes.Status401Unauthorized);

                if (!user.IsActive)
                    return GenericResponse<UserResponseDTO>.Error("User account is locked. Please contact administration.", StatusCodes.Status423Locked);

                var userIsAuthenticated = await _userManager.CheckPasswordAsync(user, userLoginDTO.Password);

                if (!userIsAuthenticated)
                    return GenericResponse<UserResponseDTO>.Error("Invalid Credentials.", StatusCodes.Status401Unauthorized);

                var token = await GenerateTokenAsync(user);
                return GenericResponse<UserResponseDTO>.Success(
                    new UserResponseDTO(token, user.UserName!, user.Email!),
                    "User logged in successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging in.");
                return GenericResponse<UserResponseDTO>.Failure("An error occurred while logging in.");
            }
        }

        public async Task<GenericResponse<UserResponseDTO>> RegisterAsync(UserRegisterDTO userRegisterDTO)
        {
            try
            {
                if (userRegisterDTO is null)
                    return GenericResponse<UserResponseDTO>.Error("Invalid user data", StatusCodes.Status400BadRequest);

                var userWithThisEmail = await _userManager.FindByEmailAsync(userRegisterDTO.Email);

                if (userWithThisEmail is not null)
                    return GenericResponse<UserResponseDTO>.Error("Email already exists, please add a new one", StatusCodes.Status409Conflict);

                var userToAdd = _mapper.Map<HotelUser>(userRegisterDTO);
                var result = await _userManager.CreateAsync(userToAdd, userRegisterDTO.Password);

                if (!result.Succeeded)
                    return GenericResponse<UserResponseDTO>.Error(string.Join(", ", result.Errors.Select(e => e.Description)), StatusCodes.Status400BadRequest);

                await _userManager.AddToRoleAsync(userToAdd, "Guest");
                var token = await GenerateTokenAsync(userToAdd);

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

                return GenericResponse<UserResponseDTO>.Success(
                    new UserResponseDTO(token, userToAdd.UserName!, userToAdd.Email!),
                    "Account created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering the user.");
                return GenericResponse<UserResponseDTO>.Failure("An error occurred while creating the account.");
            }
        }

        public async Task<GenericResponse<bool>> CreateStaffAccountAsync(StaffCreationDTO staffCreationDTO)
        {
            try
            {
                if (staffCreationDTO is null)
                    return GenericResponse<bool>.Error("Invalid staff data", StatusCodes.Status400BadRequest);

                var userWithThisEmail = await _userManager.FindByEmailAsync(staffCreationDTO.Email);
                if (userWithThisEmail is not null)
                    return GenericResponse<bool>.Error("Email already exists, please add a new one", StatusCodes.Status409Conflict);

                var staffSpeciality = AuthServiceHelper.GetStaffSpeciality(staffCreationDTO.Specialty);
                if (staffSpeciality is null)
                    return GenericResponse<bool>.Error("Invalid staff specialty.", StatusCodes.Status400BadRequest);

                var staffToAdd = _mapper.Map<StaffUser>(staffCreationDTO);
                staffToAdd.UserName = staffCreationDTO.Email;
                staffToAdd.Specialities = staffSpeciality.Value;

                var createResult = await _userManager.CreateAsync(staffToAdd, staffCreationDTO.Password);
                if (!createResult.Succeeded)
                    return GenericResponse<bool>.Error(string.Join(", ", createResult.Errors.Select(e => e.Description)), StatusCodes.Status400BadRequest);

                var roleResult = await _userManager.AddToRoleAsync(staffToAdd, "Staff");
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(staffToAdd);
                    return GenericResponse<bool>.Failure("Failed to assign Staff role. The user was not created.");
                }

                return GenericResponse<bool>.Success(true, "Staff account created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a staff account.");
                return GenericResponse<bool>.Failure("An error occurred while creating the staff account.");
            }
        }

        public async Task<GenericResponse<bool>> DeactivateUserAsync(string userId)
            => await SetUserActiveStateAsync(userId, isActive: false);

        public async Task<GenericResponse<bool>> ActivateUserAsync(string userId)
            => await SetUserActiveStateAsync(userId, isActive: true);

        public async Task<GenericResponse<bool>> CheckEmailExistsAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return GenericResponse<bool>.Error("Email is required.", StatusCodes.Status400BadRequest);

                var user = await _userManager.FindByEmailAsync(email);
                return GenericResponse<bool>.Success(user is not null, "Email check completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking email existence.");
                return GenericResponse<bool>.Failure("An error occurred while checking the email.");
            }
        }

        public async Task<GenericResponse<IEnumerable<UserInfoDTO>>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();

            if (users is null || !users.Any())
                return GenericResponse<IEnumerable<UserInfoDTO>>.Error("No users found.", StatusCodes.Status404NotFound);

            var userInfoDTOs = _mapper.Map<IEnumerable<UserInfoDTO>>(users);

            return GenericResponse<IEnumerable<UserInfoDTO>>.Success(userInfoDTOs, "Users retrieved successfully.");
        }

        public async Task<GenericResponse<UserInfoDTO>> GetUserInfoAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return GenericResponse<UserInfoDTO>.Error("User id is required.", StatusCodes.Status400BadRequest);

            var user = await _userManager.Users.Include(user => user.GuestBookings).FirstOrDefaultAsync(user => user.Id == userId);

            if (user is null)
                return GenericResponse<UserInfoDTO>.Error("User not found.", StatusCodes.Status404NotFound);

            var userInfoDTO = _mapper.Map<UserInfoDTO>(user);

            return GenericResponse<UserInfoDTO>.Success(userInfoDTO, "User information retrieved successfully.");
        }

        #region Helper Methods
        private async Task<GenericResponse<bool>> SetUserActiveStateAsync(string userId, bool isActive)
        {
            var actionName = isActive ? "activating" : "deactivating";

            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return GenericResponse<bool>.Error("User id is required.", StatusCodes.Status400BadRequest);

                var user = await _userManager.FindByIdAsync(userId);
                if (user is null)
                    return GenericResponse<bool>.Error("User was not found.", StatusCodes.Status404NotFound);

                user.IsActive = isActive;
                user.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    return GenericResponse<bool>.Error(string.Join(", ", updateResult.Errors.Select(e => e.Description)), StatusCodes.Status400BadRequest);

                return GenericResponse<bool>.Success(true, isActive ? "User activated successfully." : "User deactivated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while {Action} user with id {UserId}.", actionName, userId);
                return GenericResponse<bool>.Failure($"An error occurred while {actionName} the user.");
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

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
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