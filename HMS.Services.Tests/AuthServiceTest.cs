using AutoMapper;
using FluentAssertions;
using HMS.Core.Entities.Enums.SecurityEnums;
using HMS.Core.Entities.SecurityModule;
using HMS.Services.Abstraction;
using HMS.Shared.DTOs.SecurityModuleDTOs;
using HMS.Shared.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace HMS.Services.Tests
{
    public class AuthServiceTest
    {
        private readonly Mock<IUserStore<HotelUser>> _mockUserStore;
        private readonly Mock<UserManager<HotelUser>> _mockUserManager;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly AuthService _authService;

        public AuthServiceTest()
        {
            #region Mock Objects Creation
            _mockUserStore = new Mock<IUserStore<HotelUser>>();
            _mockUserManager = new Mock<UserManager<HotelUser>>(
                _mockUserStore.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);
            _mockConfiguration = new Mock<IConfiguration>();
            _mockMapper = new Mock<IMapper>();
            _mockEmailService = new Mock<IEmailService>();
            _mockLogger = new Mock<ILogger<AuthService>>();

            _authService = new AuthService(
                _mockUserManager.Object,
                _mockConfiguration.Object,
                _mockMapper.Object,
                _mockEmailService.Object,
                _mockLogger.Object);
            #endregion

            #region Mocks Global Setup
            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(section => section["SecretKey"]).Returns("ThisIsASecretKeyForJwtTokenGeneration123!");
            jwtSection.Setup(section => section["Issuer"]).Returns("HMS");
            jwtSection.Setup(section => section["Audience"]).Returns("HMSUsers");
            _mockConfiguration.Setup(config => config.GetSection("JWTOptions")).Returns(jwtSection.Object);

            _mockUserManager.Setup(um => um.GetRolesAsync(It.IsAny<HotelUser>()))
                .ReturnsAsync(new List<string> { "Guest" });

            _mockEmailService.Setup(es => es.SendEmailAsync(It.IsAny<Email>()))
                .Returns(Task.CompletedTask);
            #endregion
        }

        #region RegisterAsync Tests
        // Happy Scenario
        [Fact]
        public async Task RegisterAsync_WhenValidData_Returns200Ok()
        {
            // Arrange
            var registerDto = new UserRegisterDTO
            {
                FullName = "John Doe",
                Email = "john@test.com",
                Password = "P@ssw0rd!",
                PhoneNumber = "01234567890"
            };

            var userToAdd = new HotelUser
            {
                Id = "user-1",
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                UserName = "john",
                IsActive = true
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync((HotelUser?)null);

            _mockMapper.Setup(am => am.Map<HotelUser>(registerDto)).Returns(userToAdd);

            _mockUserManager.Setup(um => um.CreateAsync(userToAdd, registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(um => um.AddToRoleAsync(userToAdd, "Guest"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().NotBeNull();
            result.Data!.Email.Should().Be(registerDto.Email);
            result.Data.Token.Should().NotBeNullOrWhiteSpace();
            result.Message.Should().Be("Account created successfully.");
            _mockUserManager.Verify(um => um.AddToRoleAsync(userToAdd, "Guest"), Times.Once);
        }

        // Edge Scenario
        [Fact]
        public async Task RegisterAsync_WhenEmailAlreadyExists_Returns409Conflict()
        {
            // Arrange
            var registerDto = new UserRegisterDTO
            {
                FullName = "John Doe",
                Email = "existing@test.com",
                Password = "P@ssw0rd!",
                PhoneNumber = "01234567890"
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync(new HotelUser { Email = registerDto.Email });

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status409Conflict);
            result.Data.Should().BeNull();
            result.Message.Should().Be("Email already exists, please add a new one");
        }

        // Edge Scenario
        [Fact]
        public async Task RegisterAsync_WhenDtoIsNull_Returns400BadRequest()
        {
            // Arrange
            UserRegisterDTO? registerDto = null;

            // Act
            var result = await _authService.RegisterAsync(registerDto!);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            result.Data.Should().BeNull();
            result.Message.Should().Be("Invalid user data");
        }
        #endregion

        #region LoginAsync Tests
        // Happy Scenario
        [Fact]
        public async Task LoginAsync_WhenValidCredentialsAndActiveUser_Returns200OkWithToken()
        {
            // Arrange
            var loginDto = new UserLoginDTO
            {
                Email = "john@test.com",
                Password = "P@ssw0rd!"
            };

            var existingUser = new HotelUser
            {
                Id = "user-1",
                Email = loginDto.Email,
                UserName = "john",
                IsActive = true
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(existingUser);

            _mockUserManager.Setup(um => um.CheckPasswordAsync(existingUser, loginDto.Password))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().NotBeNull();
            result.Data!.Token.Should().NotBeNullOrWhiteSpace();
            result.Data.Email.Should().Be(loginDto.Email);
            result.Message.Should().Be("User logged in successfully.");
        }

        // Edge Scenario
        [Fact]
        public async Task LoginAsync_WhenUserNotFound_Returns401Unauthorized()
        {
            // Arrange
            var loginDto = new UserLoginDTO
            {
                Email = "missing@test.com",
                Password = "P@ssw0rd!"
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync((HotelUser?)null);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            result.Data.Should().BeNull();
            result.Message.Should().Be("Invalid Credentials.");
        }

        // Edge Scenario
        [Fact]
        public async Task LoginAsync_WhenAccountIsInactive_Returns423Locked()
        {
            // Arrange
            var loginDto = new UserLoginDTO
            {
                Email = "locked@test.com",
                Password = "P@ssw0rd!"
            };

            var inactiveUser = new HotelUser
            {
                Id = "user-2",
                Email = loginDto.Email,
                UserName = "locked",
                IsActive = false
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(inactiveUser);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status423Locked);
            result.Data.Should().BeNull();
            result.Message.Should().Be("User account is locked. Please contact administration.");
        }

        // Edge Scenario
        [Fact]
        public async Task LoginAsync_WhenDtoIsNull_Returns400BadRequest()
        {
            // Arrange
            UserLoginDTO? loginDto = null;

            // Act
            var result = await _authService.LoginAsync(loginDto!);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            result.Data.Should().BeNull();
            result.Message.Should().Be("No data was sent.");
        }

        // Edge Scenario
        [Fact]
        public async Task LoginAsync_WhenPasswordIsInvalid_Returns401Unauthorized()
        {
            // Arrange
            var loginDto = new UserLoginDTO
            {
                Email = "john@test.com",
                Password = "WrongPassword"
            };

            var existingUser = new HotelUser
            {
                Id = "user-1",
                Email = loginDto.Email,
                UserName = "john",
                IsActive = true
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(existingUser);

            _mockUserManager.Setup(um => um.CheckPasswordAsync(existingUser, loginDto.Password))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            result.Data.Should().BeNull();
            result.Message.Should().Be("Invalid Credentials.");
        }
        #endregion

        #region CreateStaffAccountAsync Tests
        // Happy Scenario
        [Fact]
        public async Task CreateStaffAccountAsync_WhenValidData_Returns200Ok()
        {
            // Arrange
            var staffDto = new StaffCreationDTO
            {
                FullName = "Staff Member",
                Email = "staff@test.com",
                Password = "P@ssw0rd!",
                PhoneNumber = "01234567890",
                Specialty = "HouseKeeping"
            };

            var staffToAdd = new StaffUser
            {
                Id = "staff-1",
                FullName = staffDto.FullName,
                Email = staffDto.Email,
                UserName = staffDto.Email,
                IsActive = true,
                Specialities = StaffSpecialities.HouseKeeping
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(staffDto.Email))
                .ReturnsAsync((HotelUser?)null);

            _mockMapper.Setup(am => am.Map<StaffUser>(staffDto)).Returns(staffToAdd);

            _mockUserManager.Setup(um => um.CreateAsync(staffToAdd, staffDto.Password))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(um => um.AddToRoleAsync(staffToAdd, "Staff"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.CreateStaffAccountAsync(staffDto);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().BeTrue();
            result.Message.Should().Be("Staff account created successfully.");
            _mockUserManager.Verify(um => um.AddToRoleAsync(staffToAdd, "Staff"), Times.Once);
        }

        // Edge Scenario
        [Fact]
        public async Task CreateStaffAccountAsync_WhenSpecialtyIsInvalid_Returns400BadRequest()
        {
            // Arrange
            var staffDto = new StaffCreationDTO
            {
                FullName = "Staff Member",
                Email = "staff@test.com",
                Password = "P@ssw0rd!",
                PhoneNumber = "01234567890",
                Specialty = "InvalidSpecialty"
            };


            _mockUserManager.Setup(um => um.FindByEmailAsync(staffDto.Email))
                .ReturnsAsync((HotelUser?)null);

            // Act
            var result = await _authService.CreateStaffAccountAsync(staffDto);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            result.Data.Should().BeFalse();
            result.Message.Should().Be("Invalid staff specialty.");
        }

        // Edge Scenario
        [Fact]
        public async Task CreateStaffAccountAsync_WhenEmailAlreadyExists_Returns409Conflict()
        {
            // Arrange
            var staffDto = new StaffCreationDTO
            {
                FullName = "Staff Member",
                Email = "existing@test.com",
                Password = "P@ssw0rd!",
                PhoneNumber = "01234567890",
                Specialty = "Laundry"
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(staffDto.Email))
                .ReturnsAsync(new HotelUser { Email = staffDto.Email });

            // Act
            var result = await _authService.CreateStaffAccountAsync(staffDto);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status409Conflict);
            result.Data.Should().BeFalse();
            result.Message.Should().Be("Email already exists, please add a new one");
        }

        // Edge Scenario
        [Fact]
        public async Task CreateStaffAccountAsync_WhenDtoIsNull_Returns400BadRequest()
        {
            // Arrange
            StaffCreationDTO? staffDto = null;

            // Act
            var result = await _authService.CreateStaffAccountAsync(staffDto!);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            result.Data.Should().BeFalse();
            result.Message.Should().Be("Invalid staff data");
        }
        #endregion

        #region DeactivateUserAsync / ActivateUserAsync Tests
        // Happy Scenario
        [Fact]
        public async Task DeactivateUserAsync_WhenUserExists_Returns200Ok()
        {
            // Arrange
            var userId = "user-1";
            var existingUser = new HotelUser
            {
                Id = userId,
                Email = "john@test.com",
                IsActive = true
            };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(existingUser);

            _mockUserManager.Setup(um => um.UpdateAsync(existingUser))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.DeactivateUserAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().BeTrue();
            result.Message.Should().Be("User deactivated successfully.");
            existingUser.IsActive.Should().BeFalse();
        }

        // Happy Scenario
        [Fact]
        public async Task ActivateUserAsync_WhenUserExists_Returns200Ok()
        {
            // Arrange
            var userId = "user-1";
            var existingUser = new HotelUser
            {
                Id = userId,
                Email = "john@test.com",
                IsActive = false
            };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(existingUser);

            _mockUserManager.Setup(um => um.UpdateAsync(existingUser))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.ActivateUserAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().BeTrue();
            result.Message.Should().Be("User activated successfully.");
            existingUser.IsActive.Should().BeTrue();
        }

        // Edge Scenario
        [Fact]
        public async Task DeactivateUserAsync_WhenUserNotFound_Returns404NotFound()
        {
            // Arrange
            var userId = "missing-user";
            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync((HotelUser?)null);

            // Act
            var result = await _authService.DeactivateUserAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            result.Data.Should().BeFalse();
            result.Message.Should().Be("User was not found.");
        }

        // Edge Scenario
        [Fact]
        public async Task ActivateUserAsync_WhenUserNotFound_Returns404NotFound()
        {
            // Arrange
            var userId = "missing-user";
            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync((HotelUser?)null);

            // Act
            var result = await _authService.ActivateUserAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            result.Data.Should().BeFalse();
            result.Message.Should().Be("User was not found.");
        }
        #endregion

        #region CheckEmailExistsAsync Tests
        [Fact]
        public async Task CheckEmailExistsAsync_WhenEmailExists_Returns200OkWithDataTrue()
        {
            // Arrange
            var email = "john@test.com";
            _mockUserManager.Setup(um => um.FindByEmailAsync(email))
                .ReturnsAsync(new HotelUser { Email = email });

            // Act
            var result = await _authService.CheckEmailExistsAsync(email);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task CheckEmailExistsAsync_WhenEmailDoesNotExist_Returns200OkWithDataFalse()
        {
            // Arrange
            var email = "missing@test.com";
            _mockUserManager.Setup(um => um.FindByEmailAsync(email))
                .ReturnsAsync((HotelUser?)null);

            // Act
            var result = await _authService.CheckEmailExistsAsync(email);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().BeFalse();
        }

        [Fact]
        public async Task CheckEmailExistsAsync_WhenEmailIsNull_Returns400BadRequest()
        {
            // Arrange
            string? email = null;

            // Act
            var result = await _authService.CheckEmailExistsAsync(email!);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            result.Data.Should().BeFalse();
            result.Message.Should().Be("Email is required.");
        }
        #endregion
    }
}
