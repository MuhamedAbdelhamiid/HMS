using AutoMapper;
using FluentAssertions;
using HMS.Core.Contracts;
using HMS.Core.Entities.Enums.RoomEnums;
using HMS.Core.Entities.RoomModuleEntities;
using HMS.Shared;
using HMS.Shared.DTOs.RoomModuleDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Linq.Expressions;

namespace HMS.Services.Tests
{
    public class RoomServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IGenericRepository<Room, int>> _mockRoomRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly RoomService _roomService;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public RoomServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _roomService = new RoomService(_mockUnitOfWork.Object, _mockMapper.Object);
            _mockConfiguration = new Mock<IConfiguration>();
            _mockRoomRepo = new Mock<IGenericRepository<Room, int>>();

            _mockUnitOfWork.Setup(_mockUnitOfWork => _mockUnitOfWork.GetRepository<Room, int>()).Returns(_mockRoomRepo.Object);

            _mockConfiguration.Setup(config => config.GetSection("Urls")["BaseUrl"]).Returns("https://localhost:7059/");
        }

        #region Guest Services Test

        #region GetAllRoomsAsync 
        // Happy Scenario
        [Fact]
        public async Task GetAllRoomsAsync_WhenRoomsExist_Returns200Ok()
        {
            //Arrange
            var fakeRooms = new List<Room>
            {
               new Room { Id = 1, Status = RoomStatus.Available, PricePerNight = 100},
               new Room { Id = 2, Status = RoomStatus.Reserved, PricePerNight = 150}
            };

            var fakeRoomsDTOs = new List<RoomDTO>()
            {
                new RoomDTO {Id = 1, PricePerNight = 100},
                new RoomDTO {Id = 2, PricePerNight = 150}
            };

            _mockRoomRepo.Setup(repo => repo.GetAllAsync(
                     It.IsAny<Expression<Func<Room, bool>>>(),
                     It.IsAny<Expression<Func<Room, object>>>(),
                     It.IsAny<Expression<Func<Room, object>>>()
                 )).ReturnsAsync(fakeRooms);

            _mockMapper.Setup(am => am.Map<IEnumerable<RoomDTO>>(
                It.IsAny<IEnumerable<Room>>())).Returns(fakeRoomsDTOs);

            var queryParameters = new RoomQueryParameters();

            // Act
            var result = await _roomService.GetAllRoomsAsync(queryParameters);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().BeEquivalentTo(fakeRoomsDTOs);
            result.Data.Should().HaveCount(2);
        }

        // Edge Scenario
        [Fact]
        public async Task GetAllRoomsAsync_WhenRoomsNotExist_Returns404NotFound()
        {
            var emptyRoomsList = new List<Room>();

            _mockRoomRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(emptyRoomsList);

            var queryParams = new RoomQueryParameters();

            var result = await _roomService.GetAllRoomsAsync(queryParams);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            result.Data.Should().BeNull();
        }

        // Filter Scenario
        [Fact]
        public async Task GetAllRoomsAsync_WhenQueryParamsApplied_ReturnsWantedFilter()
        {
            var fakeRooms = new List<Room>
            {
               new Room { Id = 3,RoomType = RoomType.Single, Status = RoomStatus.Available, PricePerNight = 70},
               new Room { Id = 1,RoomType = RoomType.Single, Status = RoomStatus.Available, PricePerNight = 100},
               new Room { Id = 2,RoomType = RoomType.Double, Status = RoomStatus.Reserved, PricePerNight = 150}
            };

            var fakeRoomsDTOs = new List<RoomDTO>()
            {
                new RoomDTO {Id = 3, PricePerNight = 70, RoomType = RoomType.Single.ToString()},
                new RoomDTO {Id = 1, PricePerNight = 100,RoomType = RoomType.Single.ToString()}
            };

            var queryParams = new RoomQueryParameters
            {
                RoomType = RoomType.Single.ToString(),
                MaxPrice = 100,
                Sort = "priceAsc"
            };

            _mockRoomRepo.Setup(repo => repo.GetAllAsync(
                     It.IsAny<Expression<Func<Room, bool>>>(),
                     It.IsAny<Expression<Func<Room, object>>>(),
                     It.IsAny<Expression<Func<Room, object>>>()
                 )).ReturnsAsync(fakeRooms);

            _mockMapper.Setup(am => am.Map<IEnumerable<RoomDTO>>(It.IsAny<IEnumerable<Room>>())).Returns(fakeRoomsDTOs);

            var result = await _roomService.GetAllRoomsAsync(queryParams);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.Should().BeInAscendingOrder(r => r.PricePerNight);
        }
        #endregion

        #region GetRoomByIdAsync
        [Fact]
        public async Task GetRoomByIdAsync_WhenRoomExist_ReturnsRoomLoadedWithImages()
        {
            var baseUrl = _mockConfiguration.Object.GetSection("Urls")["BaseUrl"];
            var room = new Room
            {
                Id = 1,
                RoomType = RoomType.Single,
                Status = RoomStatus.Available,
                PricePerNight = 100,
                Images = new List<RoomImage>
                {
                    new RoomImage { Id = 1, ImageUrl = "images/rooms/image1.jpg" },
                    new RoomImage { Id = 2, ImageUrl = "images/rooms/image2.jpg" }
                }
            };

            var roomDTO = new RoomDetailsDTO
            {
                Id = 1,
                RoomType = RoomType.Single.ToString(),
                Status = RoomStatus.Available.ToString(),
                PricePerNight = 100,
                Images = new List<string>
                {
                    $"{baseUrl}images/rooms/image1.jpg",
                    $"{baseUrl}images/rooms/image2.jpg"
                }
            };

            _mockRoomRepo.Setup(repo => repo.GetByIdAsync(
                room.Id,
                It.IsAny<Expression<Func<Room, object>>>()
                )).ReturnsAsync(room);

            _mockMapper.Setup(am => am.Map<RoomDetailsDTO>(It.IsAny<Room>())).Returns(roomDTO);

            var result = await _roomService.GetRoomByIdAsync(room.Id);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().NotBeNull();
            result.Data.Should().BeEquivalentTo(roomDTO);
        }

        [Fact]
        public async Task GetRoomByIdAsync_WhenRoomNotExist_Return404NotFound()
        {
            _mockRoomRepo.Setup(repo => repo.GetByIdAsync(1, It.IsAny<Expression<Func<Room, object>>>())).ReturnsAsync((Room)null!);

            var result = await _roomService.GetRoomByIdAsync(1);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            result.Data.Should().BeNull();
        }
        [Fact]
        public async Task GetRoomByIdAsync_WhenRoomInMaintenance_Return404NotFound()
        {
            var InMaintenanceRoom = new Room
            {
                Id = 1,
                Status = RoomStatus.Maintenance
            };
            _mockRoomRepo.Setup(repo => repo.GetByIdAsync(1, It.IsAny<Expression<Func<Room, object>>>())).ReturnsAsync(InMaintenanceRoom);

            var result = await _roomService.GetRoomByIdAsync(1);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            result.Data.Should().BeNull();
            result.Message.Should().Be("Room with id: 1 in maintenance.");
        }

        #endregion 

        #endregion
    }
}
