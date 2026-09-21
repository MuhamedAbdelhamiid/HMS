using AutoMapper;
using FluentAssertions;
using HMS.Core.Contracts;
using HMS.Core.Entities.BookingModule;
using HMS.Core.Entities.Enums.BookingEnums;
using HMS.Core.Entities.ModerationModule;
using HMS.Services.Abstraction;
using HMS.Shared.DTOs.ModerationModuleDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace HMS.Services.Tests
{
    public class FeedbackServiceTest
    {
        private readonly Mock<IAiModerationService> _mockModerationService;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<FeedbackService>> _mockLogger;
        private readonly Mock<IGenericRepository<Feedback, Guid>> _mockFeedbackRepo;
        private readonly Mock<IGenericRepository<BookingEntity, Guid>> _mockBookingRepo;
        private readonly FeedbackService _feedbackService;

        public FeedbackServiceTest()
        {
            _mockModerationService = new Mock<IAiModerationService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<FeedbackService>>();
            _mockFeedbackRepo = new Mock<IGenericRepository<Feedback, Guid>>();
            _mockBookingRepo = new Mock<IGenericRepository<BookingEntity, Guid>>();

            _mockUnitOfWork
                .Setup(u => u.GetRepository<Feedback, Guid>())
                .Returns(_mockFeedbackRepo.Object);

            _mockUnitOfWork
                .Setup(u => u.GetRepository<BookingEntity, Guid>())
                .Returns(_mockBookingRepo.Object);

            _feedbackService = new FeedbackService(
                _mockModerationService.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockLogger.Object);
        }

        private static BookingEntity CreateValidBooking(Guid bookingId, string userId) => new BookingEntity
        {
            Id = bookingId,
            HotelUserId = userId,
            Status = BookingStatus.Paid,
            CheckInDate = DateTime.UtcNow.AddDays(1),
            CheckOutDate = DateTime.UtcNow.AddDays(3)
        };

        #region CreateFeedbackAsync Tests

        [Fact]
        public async Task CreateFeedbackAsync_WhenBookingNotFound_Returns404()
        {
            var userId = "user123";
            var dto = new CreateFeedbackDTO
            {
                BookingId = Guid.NewGuid(),
                Comment = "Great stay!"
            };

            _mockBookingRepo
                .Setup(r => r.GetByIdAsync(dto.BookingId))
                .ReturnsAsync((BookingEntity)null!);

            var result = await _feedbackService.CreateFeedbackAsync(userId, dto);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            result.Message.Should().Be("Booking not found.");
        }

        [Fact]
        public async Task CreateFeedbackAsync_WhenBookingNotValid_Returns402()
        {
            var userId = "user123";
            var dto = new CreateFeedbackDTO
            {
                BookingId = Guid.NewGuid(),
                Comment = "Great stay!"
            };

            var booking = new BookingEntity
            {
                Id = dto.BookingId,
                HotelUserId = userId,
                Status = BookingStatus.Pending,
                CheckInDate = DateTime.UtcNow.AddDays(1),
                CheckOutDate = DateTime.UtcNow.AddDays(3)
            };

            _mockBookingRepo
                .Setup(r => r.GetByIdAsync(dto.BookingId))
                .ReturnsAsync(booking);

            var result = await _feedbackService.CreateFeedbackAsync(userId, dto);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status402PaymentRequired);
            result.Message.Should().Be("Booking not valid to create a feedback for.");
        }

        [Fact]
        public async Task CreateFeedbackAsync_WhenUserNotOwnerOfBooking_Returns401()
        {
            var userId = "user123";
            var dto = new CreateFeedbackDTO
            {
                BookingId = Guid.NewGuid(),
                Comment = "Great stay!"
            };

            var booking = CreateValidBooking(dto.BookingId, "differentUser456");

            _mockBookingRepo
                .Setup(r => r.GetByIdAsync(dto.BookingId))
                .ReturnsAsync(booking);

            var result = await _feedbackService.CreateFeedbackAsync(userId, dto);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            result.Message.Should().Be("User unauthorized to feedback this booking.");
        }

        [Fact]
        public async Task CreateFeedbackAsync_WhenCommentIsEmpty_Returns400()
        {
            var userId = "user123";
            var dto = new CreateFeedbackDTO
            {
                BookingId = Guid.NewGuid(),
                Comment = "   "
            };

            var booking = CreateValidBooking(dto.BookingId, userId);

            _mockBookingRepo
                .Setup(r => r.GetByIdAsync(dto.BookingId))
                .ReturnsAsync(booking);

            var result = await _feedbackService.CreateFeedbackAsync(userId, dto);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            result.Message.Should().Be("Comment can not be empty");
        }

        [Fact]
        public async Task CreateFeedbackAsync_WhenCommentRejectedByModeration_Returns400()
        {
            var userId = "user123";
            var dto = new CreateFeedbackDTO
            {
                BookingId = Guid.NewGuid(),
                Comment = "Offensive comment"
            };

            var booking = CreateValidBooking(dto.BookingId, userId);

            _mockBookingRepo
                .Setup(r => r.GetByIdAsync(dto.BookingId))
                .ReturnsAsync(booking);

            _mockModerationService
                .Setup(m => m.IsUserCommentAcceptedAsFeedbackAsync(dto.Comment))
                .ReturnsAsync(false);

            var result = await _feedbackService.CreateFeedbackAsync(userId, dto);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            result.Message.Should().Be("Comment not accepted as feedback.");

            _mockFeedbackRepo.Verify(r => r.AddAsync(It.IsAny<Feedback>()), Times.Never);
        }

        [Fact]
        public async Task CreateFeedbackAsync_WhenAllValid_Returns201AndSavesFeedback()
        {
            var userId = "user123";
            var dto = new CreateFeedbackDTO
            {
                BookingId = Guid.NewGuid(),
                Comment = "Great stay, loved the service!"
            };

            var booking = CreateValidBooking(dto.BookingId, userId);

            var mappedFeedback = new Feedback
            {
                Id = Guid.NewGuid(),
                BookingId = dto.BookingId,
                Comment = dto.Comment
            };

            _mockBookingRepo
                .Setup(r => r.GetByIdAsync(dto.BookingId))
                .ReturnsAsync(booking);

            _mockModerationService
                .Setup(m => m.IsUserCommentAcceptedAsFeedbackAsync(dto.Comment))
                .ReturnsAsync(true);

            _mockMapper
                .Setup(m => m.Map<Feedback>(dto))
                .Returns(mappedFeedback);

            _mockFeedbackRepo
                .Setup(r => r.AddAsync(mappedFeedback))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            var result = await _feedbackService.CreateFeedbackAsync(userId, dto);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status201Created);
            result.Data.Should().BeTrue();
            result.Message.Should().Be("Feedback created successfully.");

            _mockFeedbackRepo.Verify(r => r.AddAsync(mappedFeedback), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateFeedbackAsync_WhenSaveThrowsException_ReturnsFailure()
        {
            var userId = "user123";
            var dto = new CreateFeedbackDTO
            {
                BookingId = Guid.NewGuid(),
                Comment = "Great stay!"
            };

            var booking = CreateValidBooking(dto.BookingId, userId);

            var mappedFeedback = new Feedback
            {
                Id = Guid.NewGuid(),
                BookingId = dto.BookingId,
                Comment = dto.Comment
            };

            _mockBookingRepo
                .Setup(r => r.GetByIdAsync(dto.BookingId))
                .ReturnsAsync(booking);

            _mockModerationService
                .Setup(m => m.IsUserCommentAcceptedAsFeedbackAsync(dto.Comment))
                .ReturnsAsync(true);

            _mockMapper
                .Setup(m => m.Map<Feedback>(dto))
                .Returns(mappedFeedback);

            _mockFeedbackRepo
                .Setup(r => r.AddAsync(mappedFeedback))
                .ThrowsAsync(new Exception("Database connection failed"));

            var result = await _feedbackService.CreateFeedbackAsync(userId, dto);

            result.Should().NotBeNull();
            result.Message.Should().Be("Unhandled exception happened while saving feedback in database.");

            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        #endregion

        #region GetAllFeedbacksAsync Tests

        [Fact]
        public async Task GetAllFeedbacksAsync_WhenNoFeedbacksExist_Returns404()
        {
            _mockFeedbackRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Feedback>());

            var result = await _feedbackService.GetAllFeedbacksAsync();

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            result.Message.Should().Be("Feedbacks not found.");
        }

        [Fact]
        public async Task GetAllFeedbacksAsync_WhenFeedbacksIsNull_Returns404()
        {
            _mockFeedbackRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync((List<Feedback>)null!);

            var result = await _feedbackService.GetAllFeedbacksAsync();

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            result.Message.Should().Be("Feedbacks not found.");
        }

        [Fact]
        public async Task GetAllFeedbacksAsync_WhenFeedbacksExist_ReturnsMappedList()
        {
            var feedbacks = new List<Feedback>
            {
                new Feedback { Id = Guid.NewGuid(), Comment = "Nice place" },
                new Feedback { Id = Guid.NewGuid(), Comment = "Will come again" }
            };

            var feedbackDtos = new List<FeedbackDTO>
            {
                new FeedbackDTO { Comment = "Nice place" },
                new FeedbackDTO { Comment = "Will come again" }
            };

            _mockFeedbackRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(feedbacks);

            _mockMapper
                .Setup(m => m.Map<IEnumerable<FeedbackDTO>>(feedbacks))
                .Returns(feedbackDtos);

            var result = await _feedbackService.GetAllFeedbacksAsync();

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().BeEquivalentTo(feedbackDtos);
            result.Message.Should().Be("Feedbacks retrieved successfully.");
        }

        #endregion
    }
}