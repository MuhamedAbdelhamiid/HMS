using AutoMapper;
using FluentAssertions;
using HMS.Core.Contracts;
using HMS.Core.Entities.BookingModule;
using HMS.Core.Entities.Enums.BookingEnums;
using HMS.Core.Entities.Enums.ServiceModule;
using HMS.Core.Entities.SecurityModule;
using HMS.Core.Entities.ServiceModule;
using HMS.Services.Abstraction;
using HMS.Shared.DTOs.ServiceModuleDTOs;
using HMS.Shared.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;


namespace HMS.Services.Tests
{
    public class RequestServiceTest
    {
        private readonly Mock<UserManager<HotelUser>> _mockUserManager;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<ILogger<RequestService>> _mockLogger;
        private readonly RequestService _requestService;

        public RequestServiceTest()
        {
            var userStore = new Mock<IUserStore<HotelUser>>();
            _mockUserManager = new Mock<UserManager<HotelUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockLogger = new Mock<ILogger<RequestService>>();

            _requestService = new RequestService(
                _mockUserManager.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockNotificationService.Object,
                _mockLogger.Object);
        }

        #region AssignStaffForRequestAsync Tests

        [Fact]
        public async Task AssignStaff_WhenStaffIdIsNull_Returns400BadRequest()
        {
            var dto = new NewAssignForStaffDTO { StaffId = "" };

            var result = await _requestService.AssignStaffForRequestAsync(Guid.NewGuid(), "admin1", dto);

            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task AssignStaff_WhenRequestNotFound_Returns404NotFound()
        {
            var dto = new NewAssignForStaffDTO { StaffId = "staff1" };
            var mockRequestRepo = new Mock<IGenericRepository<ServiceRequest, Guid>>();

            mockRequestRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<List<Expression<Func<ServiceRequest, object>>>>()))
                           .ReturnsAsync((ServiceRequest)null!);
            _mockUnitOfWork.Setup(u => u.GetRepository<ServiceRequest, Guid>()).Returns(mockRequestRepo.Object);

            var result = await _requestService.AssignStaffForRequestAsync(Guid.NewGuid(), "admin1", dto);

            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task AssignStaff_WhenRequestStatusIsNotPending_Returns400BadRequest()
        {
            var dto = new NewAssignForStaffDTO { StaffId = "staff1" };
            var request = new ServiceRequest { Status = ServiceRequestStatus.Assigned };
            var mockRequestRepo = new Mock<IGenericRepository<ServiceRequest, Guid>>();

            mockRequestRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<List<Expression<Func<ServiceRequest, object>>>>()))
                           .ReturnsAsync(request);
            _mockUnitOfWork.Setup(u => u.GetRepository<ServiceRequest, Guid>()).Returns(mockRequestRepo.Object);

            var result = await _requestService.AssignStaffForRequestAsync(Guid.NewGuid(), "admin1", dto);

            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task AssignStaff_WhenHappyPath_Returns200Ok()
        {
            var staffId = "staff123";
            var dto = new NewAssignForStaffDTO { StaffId = staffId };
            var request = new ServiceRequest
            {
                Id = Guid.NewGuid(),
                Status = ServiceRequestStatus.Pending,
                Service = new Service { Name = "Cleaning" }
            };
            var mockRequestRepo = new Mock<IGenericRepository<ServiceRequest, Guid>>();

            mockRequestRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<List<Expression<Func<ServiceRequest, object>>>>()))
                           .ReturnsAsync(request);
            _mockUnitOfWork.Setup(u => u.GetRepository<ServiceRequest, Guid>()).Returns(mockRequestRepo.Object);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            _mockUserManager.Setup(um => um.FindByIdAsync(staffId))
                            .ReturnsAsync(new StaffUser { Id = staffId, FullName = "Ahmed" });

            var result = await _requestService.AssignStaffForRequestAsync(request.Id, "admin1", dto);

            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().BeTrue();
            _mockNotificationService.Verify(n => n.NotifyStaffAssignedAsync(staffId, It.IsAny<NewAssignForStaff>()), Times.Once);
        }

        #endregion

        #region CreateServiceRequestAsync Tests

        [Fact]
        public async Task CreateRequest_WhenBookingNotFound_Returns404NotFound()
        {
            var dto = new CreateServiceRequestDTO { BookingId = Guid.NewGuid(), ServiceId = 1 };
            var mockBookingRepo = new Mock<IGenericRepository<BookingEntity, Guid>>();

            mockBookingRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<List<Expression<Func<BookingEntity, object>>>>()))
                           .ReturnsAsync((BookingEntity)null!);
            _mockUnitOfWork.Setup(u => u.GetRepository<BookingEntity, Guid>()).Returns(mockBookingRepo.Object);

            var result = await _requestService.CreateServiceRequestAsync(dto, "user1");

            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task CreateRequest_WhenBookingIsNotValid_Returns402PaymentRequired()
        {
            var dto = new CreateServiceRequestDTO { BookingId = Guid.NewGuid(), ServiceId = 1 };
            var booking = new BookingEntity
            {
                Status = BookingStatus.Pending,
                CheckInDate = DateTime.UtcNow.AddDays(1)
            };
            var mockBookingRepo = new Mock<IGenericRepository<BookingEntity, Guid>>();

            mockBookingRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<List<Expression<Func<BookingEntity, object>>>>()))
                           .ReturnsAsync(booking);
            _mockUnitOfWork.Setup(u => u.GetRepository<BookingEntity, Guid>()).Returns(mockBookingRepo.Object);

            var result = await _requestService.CreateServiceRequestAsync(dto, "user1");

            result.StatusCode.Should().Be(StatusCodes.Status402PaymentRequired);
        }

        [Fact]
        public async Task CreateRequest_WhenServiceNotFound_Returns404NotFound()
        {
            var dto = new CreateServiceRequestDTO { BookingId = Guid.NewGuid(), ServiceId = 1 };
            var booking = new BookingEntity
            {
                Status = BookingStatus.Paid,
                CheckInDate = DateTime.UtcNow.AddDays(1),
                CheckOutDate = DateTime.UtcNow.AddDays(2)
            };
            var mockBookingRepo = new Mock<IGenericRepository<BookingEntity, Guid>>();
            var mockServiceRepo = new Mock<IGenericRepository<Service, int>>();

            mockBookingRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<List<Expression<Func<BookingEntity, object>>>>()))
                           .ReturnsAsync(booking);
            mockServiceRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Service)null!);

            _mockUnitOfWork.Setup(u => u.GetRepository<BookingEntity, Guid>()).Returns(mockBookingRepo.Object);
            _mockUnitOfWork.Setup(u => u.GetRepository<Service, int>()).Returns(mockServiceRepo.Object);

            var result = await _requestService.CreateServiceRequestAsync(dto, "user1");

            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task CreateRequest_WhenHappyPath_Returns201Created()
        {
            var dto = new CreateServiceRequestDTO { BookingId = Guid.NewGuid(), ServiceId = 1 };
            var booking = new BookingEntity
            {
                Status = BookingStatus.Paid,
                CheckInDate = DateTime.UtcNow.AddDays(1),
                CheckOutDate = DateTime.UtcNow.AddDays(2),
                User = new HotelUser { FullName = "Guest" },
                RoomId = 101
            };
            var service = new Service { Id = 1, Name = "Cleaning" };
            var mappedRequest = new ServiceRequest { Id = Guid.NewGuid() };

            var mockBookingRepo = new Mock<IGenericRepository<BookingEntity, Guid>>();
            var mockServiceRepo = new Mock<IGenericRepository<Service, int>>();
            var mockRequestRepo = new Mock<IGenericRepository<ServiceRequest, Guid>>();

            mockBookingRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<List<Expression<Func<BookingEntity, object>>>>()))
                           .ReturnsAsync(booking);
            mockServiceRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(service);
            mockRequestRepo.Setup(r => r.AddAsync(It.IsAny<ServiceRequest>())).Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(u => u.GetRepository<BookingEntity, Guid>()).Returns(mockBookingRepo.Object);
            _mockUnitOfWork.Setup(u => u.GetRepository<Service, int>()).Returns(mockServiceRepo.Object);
            _mockUnitOfWork.Setup(u => u.GetRepository<ServiceRequest, Guid>()).Returns(mockRequestRepo.Object);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            _mockMapper.Setup(m => m.Map<ServiceRequest>(It.IsAny<CreateServiceRequestDTO>())).Returns(mappedRequest);

            var result = await _requestService.CreateServiceRequestAsync(dto, "user1");

            result.StatusCode.Should().Be(StatusCodes.Status201Created);
            result.Data.Should().BeTrue();
            _mockNotificationService.Verify(n => n.NotifyAdminsNewRequestAsync(It.IsAny<NewRequestMessageForAdmin>()), Times.Once);
        }

        #endregion

        #region UpdateRequestServiceAsync
        [Fact]
        public async Task UpdateRequestStatusAsync_WhenStaffIsUnauthorized_Returns401Unauthorized()
        {
            var requestId = Guid.NewGuid();
            var authorizedStaffId = "staff123";
            var unauthorizedStaffId = "staff999";
            var dto = new UpdateRequestStatusDTO { RequestId = requestId, Status = "Completed" };

            var request = new ServiceRequest
            {
                Id = requestId,
                StaffId = authorizedStaffId
            };

            var mockRequestRepo = new Mock<IGenericRepository<ServiceRequest, Guid>>();
            mockRequestRepo.Setup(r => r.GetByIdAsync(requestId)).ReturnsAsync(request);
            _mockUnitOfWork.Setup(u => u.GetRepository<ServiceRequest, Guid>()).Returns(mockRequestRepo.Object);

            var result = await _requestService.UpdateRequestStatusAsync(unauthorizedStaffId, dto);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact]
        public async Task UpdateRequestStatusAsync_WhenStatusIsInvalid_Returns400BadRequest()
        {
            var requestId = Guid.NewGuid();
            var staffId = "staff123";
            var dto = new UpdateRequestStatusDTO { RequestId = requestId, Status = "Pending" };

            var request = new ServiceRequest
            {
                Id = requestId,
                StaffId = staffId
            };

            var mockRequestRepo = new Mock<IGenericRepository<ServiceRequest, Guid>>();
            mockRequestRepo.Setup(r => r.GetByIdAsync(requestId)).ReturnsAsync(request);
            _mockUnitOfWork.Setup(u => u.GetRepository<ServiceRequest, Guid>()).Returns(mockRequestRepo.Object);

            var result = await _requestService.UpdateRequestStatusAsync(staffId, dto);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }
        #endregion
    }
}