using AutoMapper;
using FluentAssertions;
using HMS.Core.Contracts;
using HMS.Core.Entities.Enums.RoomEnums;
using HMS.Core.Entities.RoomModule;
using HMS.Services.Abstraction;
using HMS.Shared.DTOs.RoomModuleDTOs;
using HMS.Shared.QueryParameters.RoomModule;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly Mock<ILogger<RoomService>> _mockLogger;
        private readonly Mock<IAttachmentService> _mockAttachmentService;


        public RoomServiceTest()
        {
            #region Mock Objects Creation
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<RoomService>>();
            _mockAttachmentService = new Mock<IAttachmentService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockRoomRepo = new Mock<IGenericRepository<Room, int>>();


            _roomService = new RoomService(_mockUnitOfWork.Object, _mockMapper.Object, _mockLogger.Object, _mockAttachmentService.Object);


            #endregion

            #region Mocks Global Setup
            _mockUnitOfWork.Setup(_mockUnitOfWork => _mockUnitOfWork.GetRepository<Room, int>()).Returns(_mockRoomRepo.Object);

            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

            _mockConfiguration.Setup(config => config.GetSection("Urls")["BaseUrl"]).Returns("https://localhost:7059/");
            #endregion
        }

        #region Guest Services Test

        #region GetAllRoomsAsync 
        [Fact]
        public async Task GetAllRoomsAsync_WhenRoomsExistWithoutQuery_Returns200Ok()
        {
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
                 )).ReturnsAsync(fakeRooms);

            _mockMapper.Setup(am => am.Map<IEnumerable<RoomDTO>>(
                It.IsAny<IEnumerable<Room>>())).Returns(fakeRoomsDTOs);


            var result = await _roomService.GetAllRoomsAsync(null);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().BeEquivalentTo(fakeRoomsDTOs);
            result.Data.Should().HaveCount(2);
        }

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
                Images = new List<RoomImageDTO>
                {
                    new RoomImageDTO()
                    {
                        Id = 1,
                        ImageUrl = $"{baseUrl}images/rooms/image1.jpg"
                    },
                    new RoomImageDTO()
                    {
                        Id = 2,
                        ImageUrl = $"{baseUrl}images/rooms/image2.jpg"
                    }
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
                Status = RoomStatus.InMaintenance
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

        #region Admin Services Test

        #region GetAllRoomsForAdmin

        [Fact]
        public async Task GetAllRoomsForAdminAsync_WhenRoomsExistWithoutQuery_Returns200Ok()
        {
            var fakeRooms = new List<Room>
            {
               new Room { Id = 1, Status = RoomStatus.Available, PricePerNight = 100},
               new Room { Id = 2, Status = RoomStatus.Reserved, PricePerNight = 150}
            };

            var fakeRoomsDTOs = new List<AdminRoomDTO>()
            {
                new AdminRoomDTO {Id = 1, PricePerNight = 100},
                new AdminRoomDTO {Id = 2, PricePerNight = 150}
            };

            _mockRoomRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(fakeRooms);

            _mockMapper.Setup(am => am.Map<IEnumerable<AdminRoomDTO>>(fakeRooms)).Returns(fakeRoomsDTOs);

            var result = await _roomService.GetAllRoomsForAdminAsync(null);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().BeEquivalentTo(fakeRoomsDTOs);

        }

        [Fact]
        public async Task
            GetAllRoomsForAdminAsync_WhenRoomsExistWithQuery_Returns200Ok()
        {
            var fakeRooms = new List<Room>
            {
               new Room { Id = 1, Status = RoomStatus.Available,RoomType = RoomType.Single, PricePerNight = 100},
               new Room { Id = 2, Status = RoomStatus.Reserved, PricePerNight = 150,RoomType = RoomType.Double}
            };

            var fakeRoomsDTOs = new List<AdminRoomDTO>()
            {
                new AdminRoomDTO {Id = 1, PricePerNight = 100, RoomType = "single"},
            };

            _mockRoomRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(fakeRooms);

            _mockMapper.Setup(am => am.Map<IEnumerable<AdminRoomDTO>>(fakeRooms)).Returns(fakeRoomsDTOs);

            var queryParameters = new AdminRoomQueryParameters
            {
                RoomType = RoomType.Single.ToString(),
                MaxPrice = 200,
                Status = RoomStatus.Available.ToString()
            };

            var result = await _roomService.GetAllRoomsForAdminAsync(null);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().BeEquivalentTo(fakeRoomsDTOs);
        }

        [Fact]
        public async Task
            GetAllRoomsForAdminAsync_WhenRoomsNotExist_Returns404NotFound()
        {
            var emptyRoomsList = new List<Room>();

            _mockRoomRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(emptyRoomsList);


            var result = await _roomService.GetAllRoomsForAdminAsync(null);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            result.Data.Should().BeNull();
        }

        #endregion

        #region CreateRoomAsync


        [Fact]
        public async Task
            CreateRoomAsync_WhenCreationSuccessfully_Returns200Ok()
        {
            var registeredRoom = new Room
            {
                Amenities = "Wifi, TV, Air Conditioning",
                Description = "This is description for a room",
                PricePerNight = 100,
                RoomType = RoomType.Single,
            };
            var roomCreationDTO = new AdminRoomCreationDTO
            {
                Amenities = "Wifi, TV, Air Conditioning",
                Description = "This is description for a room",
                PricePerNight = 100,
                RoomType = RoomType.Single.ToString(),
            };

            _mockMapper.Setup(am => am.Map<Room>(roomCreationDTO)).Returns(registeredRoom);

            var result = await _roomService.CreateRoomAsync(roomCreationDTO);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().Be(true);
        }

        [Fact]
        public async Task
            CreateRoomAsync_WhenCreationFailedDueInvalidData_Returns400BadRequest()
        {
            var result = await _roomService.CreateRoomAsync(null!);

            result.Should().NotBeNull();
            result.Message.Should().BeEquivalentTo("Invalid room data");
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        }

        #endregion

        #region UpdateRoomAsync

        [Fact]
        public async Task UpdateRoomAsync_WhenRoomExists_Returns200Ok()
        {
            var room = new Room
            {
                Id = 1,
            };

            var roomUpdateDTO = new AdminRoomUpdateDTO
            {
                Amenities = "Wifi, TV, Air Conditioning, Mini Bar",
                Description = "This is updated description for a room",
                PricePerNight = 120,
                RoomType = RoomType.Double.ToString(),
            };

            var updatedRoom = new Room
            {
                Id = 1,
                Amenities = "Wifi, TV, Air Conditioning, Mini Bar",
                Description = "This is updated description for a room",
                PricePerNight = 120,
                RoomType = RoomType.Double,
            };

            _mockRoomRepo.Setup(repo => repo.GetByIdAsync(
                room.Id, It.IsAny<Expression<Func<Room, object>>>())).ReturnsAsync(room);

            _mockMapper.Setup(am => am.Map(roomUpdateDTO, room)).Returns(updatedRoom);

            var result = await _roomService.UpdateRoomAsync(room.Id, roomUpdateDTO);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().Be(true);
        }

        [Fact]
        public async Task UpdateRoomAsync_WhenRoomNotExist_Returns404NotFound()
        {
            var result = await _roomService.UpdateRoomAsync(0, null);


            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        #endregion

        #region DeleteRoomAsync

        [Fact]
        public async Task
            DeleteRoomAsync_WhenRoomExistAndDeletedSuccessfully_Returns200Ok()
        {
            var room = new Room
            {
                Id = 1,
                Status = RoomStatus.Available
            };

            _mockRoomRepo.Setup(repo => repo.GetByIdAsync(room.Id)).ReturnsAsync(room);

            var result = await _roomService.DeleteRoomAsync(room.Id);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task DeleteRoomAsync_WhenRoomNotExist_Returns404NotFound()
        {
            var result = await _roomService.DeleteRoomAsync(0);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task DeleteRoomAsync_WhenRoomReserved_Returns404NotFound()
        {
            var room = new Room
            {
                Id = 1,
                Status = RoomStatus.Reserved
            };
            _mockRoomRepo.Setup(repo => repo.GetByIdAsync(room.Id)).ReturnsAsync(room);

            var result = await _roomService.DeleteRoomAsync(room.Id);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        #endregion

        #region UploadRoomImageAsync
        [Fact]
        public async Task UploadRoomImageAsync_WhenRoomExistsAndUploadSucceeds_Returns200Ok()
        {
            var room = new Room { Id = 1, Images = new List<RoomImage>() };

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.jpg");
            mockFile.Setup(f => f.Length).Returns(1000);
            var filesList = new List<IFormFile> { mockFile.Object };

            _mockRoomRepo.Setup(repo => repo.GetByIdAsync(1, It.IsAny<Expression<Func<Room, object>>>()))
                         .ReturnsAsync(room);

            _mockAttachmentService.Setup(att => att.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IFormFile>()))
                                   .ReturnsAsync("rooms/test.jpg");

            var result = await _roomService.UploadRoomImageAsync(1, filesList);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task UploadRoomImageAsync_WhenAttachmentServiceFails_Returns500AndCleansUp()
        {
            // Arrange
            var room = new Room { Id = 1, Images = new List<RoomImage>() };

            var mockFile1 = new Mock<IFormFile>();
            mockFile1.Setup(f => f.FileName).Returns("test1.jpg");
            var mockFile2 = new Mock<IFormFile>();
            mockFile2.Setup(f => f.FileName).Returns("test2.jpg");

            var filesList = new List<IFormFile> { mockFile1.Object, mockFile2.Object };

            _mockRoomRepo.Setup(repo => repo.GetByIdAsync(1, It.IsAny<Expression<Func<Room, object>>>()))
                         .ReturnsAsync(room);

            _mockAttachmentService.Setup(att => att.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IFormFile>()))
                                   .ReturnsAsync((string)null!);

            var result = await _roomService.UploadRoomImageAsync(1, filesList);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);


        }

        #endregion

        #region DeleteRoomImageAsync
        [Fact]
        public async Task DeleteRoomImagesAsync_WhenRoomAndImageExist_Returns200Ok()
        {
            var imageId = 5;
            var roomImage = new RoomImage { Id = imageId, ImageUrl = "rooms/test.jpg" };
            var room = new Room { Id = 1, Images = new List<RoomImage> { roomImage } };

            _mockRoomRepo.Setup(repo => repo.GetByIdAsync(1, It.IsAny<Expression<Func<Room, object>>>()))
                         .ReturnsAsync(room);

            _mockAttachmentService.Setup(att => att.Delete("rooms/test.jpg"))
                                   .Returns(true);

            var result = await _roomService.DeleteRoomImagesAsync(1, imageId);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().BeTrue();

        }

        [Fact]
        public async Task DeleteRoomImagesAsync_WhenImageDoesNotExist_Returns400BadRequest()
        {
            var room = new Room { Id = 1, Images = new List<RoomImage>() };

            _mockRoomRepo.Setup(repo => repo.GetByIdAsync(1, It.IsAny<Expression<Func<Room, object>>>()))
                         .ReturnsAsync(room);

            var result = await _roomService.DeleteRoomImagesAsync(1, 99);

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
        #endregion

        #endregion
    }
}
