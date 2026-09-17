using AutoMapper;
using FluentAssertions;
using HMS.Core.Contracts;
using HMS.Core.Entities.BookingModule;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace HMS.Services.Tests
{
    public class BookingServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<BookingService>> _mockLogger;
        private readonly BookingService _bookingService;

        public BookingServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<BookingService>>();

            _bookingService = new BookingService(_mockUnitOfWork.Object, _mockMapper.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CancelBookingAsync_WhenBookingExistsAndFutureDate_Returns200Ok()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var booking = new BookingEntity { Id = bookingId, CheckOutDate = DateTime.UtcNow.AddDays(5) };
            var mockBookingRepo = new Mock<IGenericRepository<BookingEntity, Guid>>();

            mockBookingRepo.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
            _mockUnitOfWork.Setup(u => u.GetRepository<BookingEntity, Guid>()).Returns(mockBookingRepo.Object);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _bookingService.CancelBookingAsync(bookingId);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task CancelBookingAsync_WhenBookingNotFound_Returns404NotFound()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var mockBookingRepo = new Mock<IGenericRepository<BookingEntity, Guid>>();

            mockBookingRepo.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync((BookingEntity)null!);
            _mockUnitOfWork.Setup(u => u.GetRepository<BookingEntity, Guid>()).Returns(mockBookingRepo.Object);

            // Act
            var result = await _bookingService.CancelBookingAsync(bookingId);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task CancelBookingAsync_WhenCheckOutDateIsPast_Returns400BadRequest()
        {
            var bookingId = Guid.NewGuid();
            var booking = new BookingEntity { Id = bookingId, CheckOutDate = DateTime.UtcNow.AddDays(-2) };
            var mockBookingRepo = new Mock<IGenericRepository<BookingEntity, Guid>>();

            mockBookingRepo.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
            _mockUnitOfWork.Setup(u => u.GetRepository<BookingEntity, Guid>()).Returns(mockBookingRepo.Object);

            var result = await _bookingService.CancelBookingAsync(bookingId);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }
    }
}