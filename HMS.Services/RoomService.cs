using AutoMapper;
using HMS.Core.Contracts;
using HMS.Core.Entities.Enums.RoomEnums;
using HMS.Core.Entities.RoomModule;
using HMS.Services.Abstraction;
using HMS.Services.Helpers;
using HMS.Shared.DTOs.RoomModuleDTOs;
using HMS.Shared.QueryParameters.RoomModule;
using HMS.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace HMS.Services
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RoomService> _logger;
        private readonly IAttachmentService _attachmentService;

        public RoomService(
             IUnitOfWork unitOfWork,
             IMapper mapper,
             ILogger<RoomService> logger,
             IAttachmentService attachmentService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _attachmentService = attachmentService;
        }

        #region Guest Services
        public async Task<GenericResponse<IEnumerable<RoomDTO>>> GetAllRoomsAsync(RoomQueryParameters? queryParameters)
        {
            var roomRepo = _unitOfWork.GetRepository<Room, int>();

            IEnumerable<Room> rooms = null!;
            if (queryParameters is not null)
            {
                Expression<Func<Room, object>>? orderBy, orderByDesc;
                RoomServiceHelper.BuildSortExpression(queryParameters.Sort, out orderBy, out orderByDesc);

                rooms = await roomRepo.GetAllAsync(filter: FilterHelper.BuildFilterExpression(queryParameters),
                   orderBy: orderBy,
                   orderByDesc: orderByDesc);
            }
            else
            {
                rooms = await roomRepo.GetAllAsync();
            }

            if (!rooms.Any())
                return GenericResponse<IEnumerable<RoomDTO>>.Error("No rooms found.", StatusCodes.Status404NotFound);

            var roomsToReturn = _mapper.Map<IEnumerable<RoomDTO>>(rooms);
            return GenericResponse<IEnumerable<RoomDTO>>.Success(roomsToReturn, "Rooms retrieved successfully");
        }

        public async Task<GenericResponse<RoomDetailsDTO>> GetRoomByIdAsync(int id)
        {
            var roomsRepo = _unitOfWork.GetRepository<Room, int>();
            var room = await roomsRepo.GetByIdAsync(id, r => r.Images);

            return HandleResponseBasedOnRoomInfo(id, room);
        }

        #endregion

        #region Admin Services
        public async Task<GenericResponse<IEnumerable<AdminRoomDTO>>> GetAllRoomsForAdminAsync(AdminRoomQueryParameters? adminQueryParameters)
        {
            var roomRepo = _unitOfWork.GetRepository<Room, int>();

            IEnumerable<Room> rooms = null!;
            if (adminQueryParameters is not null)
            {
                Expression<Func<Room, object>>? orderBy, orderByDesc;
                RoomServiceHelper.BuildSortExpression(adminQueryParameters.Sort, out orderBy, out orderByDesc);

                rooms = await roomRepo.GetAllAsync(filter: FilterHelper.BuildFilterExpression(adminQueryParameters),
                   orderBy: orderBy,
                   orderByDesc: orderByDesc);
            }
            else
            {
                rooms = await roomRepo.GetAllAsync();
            }

            if (rooms is null || !rooms.Any())
                return GenericResponse<IEnumerable<AdminRoomDTO>>.Error("No rooms found.", StatusCodes.Status404NotFound);

            var roomsToReturn = _mapper.Map<IEnumerable<AdminRoomDTO>>(rooms);
            return GenericResponse<IEnumerable<AdminRoomDTO>>.Success(roomsToReturn, "Rooms retrieved successfully");
        }

        public async Task<GenericResponse<bool>> CreateRoomAsync(AdminRoomCreationDTO roomToCreate)
        {
            try
            {
                if (roomToCreate is null)
                    return GenericResponse<bool>.Error("Invalid room data", StatusCodes.Status400BadRequest);

                var roomRepo = _unitOfWork.GetRepository<Room, int>();
                var roomToAdd = _mapper.Map<Room>(roomToCreate);
                roomToAdd.RoomType = (RoomType)RoomServiceHelper.GetRoomType(roomToCreate.RoomType)!;

                await roomRepo.AddAsync(roomToAdd);
                var saved = await _unitOfWork.SaveChangesAsync() > 0;

                if (!saved)
                    return GenericResponse<bool>.Error("Failed to create the room.", StatusCodes.Status500InternalServerError);

                return GenericResponse<bool>.Success(true, "Room created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating room.");
                return GenericResponse<bool>.Failure("An error occurred while creating the room.");
            }
        }

        public async Task<GenericResponse<bool>> UpdateRoomAsync(int id, AdminRoomUpdateDTO roomToUpdate)
        {
            try
            {
                var roomRepo = _unitOfWork.GetRepository<Room, int>();
                var room = await roomRepo.GetByIdAsync(id, r => r.Images);

                if (room is null)
                    return GenericResponse<bool>.Error("Room can not be updated cause it was not found.", StatusCodes.Status404NotFound);

                room = _mapper.Map(roomToUpdate, room);
                roomRepo.Update(room);
                var saved = await _unitOfWork.SaveChangesAsync() > 0;

                if (!saved)
                    return GenericResponse<bool>.Error("Error while room update.", StatusCodes.Status500InternalServerError);

                return GenericResponse<bool>.Success(true, "Room updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There an unexpected error happened while updating on room with id: {id}.");
                return GenericResponse<bool>.Failure($"There an unexpected error happened while updating on room with id: {id}.");
            }
        }

        public async Task<GenericResponse<bool>> DeleteRoomAsync(int id)
        {
            var roomRepo = _unitOfWork.GetRepository<Room, int>();
            var room = await roomRepo.GetByIdAsync(id);

            if (room is null)
                return GenericResponse<bool>.Error($"Room with id: {id} was not found.", StatusCodes.Status404NotFound);

            return await HandleDeleteDecision(room, roomRepo);
        }

        public async Task<GenericResponse<bool>> UploadRoomImageAsync(int roomId, List<IFormFile> imageFiles)
        {
            var roomRepo = _unitOfWork.GetRepository<Room, int>();

            try
            {
                var room = await roomRepo.GetByIdAsync(roomId, r => r.Images);

                if (room is null)
                    return GenericResponse<bool>.Error($"Room with id: {roomId} was not found.", StatusCodes.Status404NotFound);

                if (imageFiles is null || !imageFiles.Any())
                    return GenericResponse<bool>.Error("No images sent.", StatusCodes.Status400BadRequest);

                var uploadedImagePaths = new List<string>();
                var imageUrls = new List<RoomImage>();

                foreach (var image in imageFiles)
                {
                    var filePath = await _attachmentService.UploadAsync("images", "rooms", image);

                    if (string.IsNullOrEmpty(filePath))
                    {
                        foreach (var imagePath in uploadedImagePaths)
                            _attachmentService.Delete(imagePath);

                        return GenericResponse<bool>.Error($"Failed to upload image: {image.FileName}", StatusCodes.Status500InternalServerError);
                    }
                    else
                    {
                        imageUrls.Add(new RoomImage() { ImageUrl = filePath });
                        uploadedImagePaths.Add(filePath);
                    }
                }

                foreach (var image in imageUrls)
                    room.Images.Add(image);

                room.UpdatedAt = DateTime.UtcNow;
                roomRepo.Update(room);
                var saved = await _unitOfWork.SaveChangesAsync() > 0;

                if (saved)
                    return GenericResponse<bool>.Success(true, "Images uploaded successfully.");

                foreach (var imagePath in uploadedImagePaths)
                    _attachmentService.Delete(imagePath);

                return GenericResponse<bool>.Error("Failed to upload images.", StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while uploading room images for room id: {roomId}");
                return GenericResponse<bool>.Failure("Failed to upload images.");
            }
        }

        public async Task<GenericResponse<bool>> DeleteRoomImagesAsync(int roomId, int imageId)
        {
            var roomRepo = _unitOfWork.GetRepository<Room, int>();

            try
            {
                var room = await roomRepo.GetByIdAsync(roomId, r => r.Images);

                if (room is null || !room.Images.Any())
                    return GenericResponse<bool>.Error($"Room with id: {roomId} was not found or have no images yet.", StatusCodes.Status404NotFound);

                var imageExists = room.Images.Any(img => img.Id == imageId);
                if (!imageExists)
                    return GenericResponse<bool>.Error($"There is file paths not exist for this room with id: {roomId}.", StatusCodes.Status400BadRequest);

                var roomImage = room.Images.FirstOrDefault(img => img.Id == imageId);
                if (roomImage is null)
                    return GenericResponse<bool>.Error($"There is no image id: {imageId} for room with id: {roomId}.", StatusCodes.Status404NotFound);

                room.Images.Remove(roomImage);
                var saved = await _unitOfWork.SaveChangesAsync() > 0;

                if (saved)
                {
                    _attachmentService.Delete(roomImage!.ImageUrl);
                    return GenericResponse<bool>.Success(true, "Images deleted successfully.");
                }

                return GenericResponse<bool>.Error($"Error occurred while saving deleting image with id: {imageId} for room id: {roomId}.", StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while saving deleting image with id: {imageId} for room id: {roomId}.");
                return GenericResponse<bool>.Failure($"Error occurred while saving deleting image with id: {imageId} for room id: {roomId}.");
            }
        }

        #endregion

        #region Helper Methods
        private GenericResponse<RoomDetailsDTO> HandleResponseBasedOnRoomInfo(int id, Room? room)
        {
            if (room is null || room.Status == RoomStatus.NotExist)
                return GenericResponse<RoomDetailsDTO>.Error($"No room found with the specified id: {id}", StatusCodes.Status404NotFound);

            if (room.Status == RoomStatus.InMaintenance)
                return GenericResponse<RoomDetailsDTO>.Error($"Room with id: {id} in maintenance.", StatusCodes.Status404NotFound);

            var roomToReturn = _mapper.Map<RoomDetailsDTO>(room);
            return GenericResponse<RoomDetailsDTO>.Success(roomToReturn, $"Room with id: {id}, retrieved successfully");
        }

        private async Task<GenericResponse<bool>> HandleDeleteDecision(Room room, IGenericRepository<Room, int> roomRepo)
        {
            switch (room.Status)
            {
                case RoomStatus.Available or RoomStatus.InMaintenance:
                    return await SoftDeleteRoomAsync(room, roomRepo);
                case RoomStatus.Reserved:
                    return GenericResponse<bool>.Error($"Room with id: {room.Id} is reserved and can not be deleted.", StatusCodes.Status400BadRequest);
                case RoomStatus.NotExist:
                    return GenericResponse<bool>.Success(true, $"Room with id: {room.Id} already soft deleted.");
                default:
                    return GenericResponse<bool>.Error("Unknown room status.", StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<GenericResponse<bool>> SoftDeleteRoomAsync(Room room, IGenericRepository<Room, int> roomRepo)
        {
            room.Status = RoomStatus.NotExist;
            room.UpdatedAt = DateTime.UtcNow;

            try
            {
                roomRepo.Update(room);
                var saved = await _unitOfWork.SaveChangesAsync() > 0;

                if (saved)
                    return GenericResponse<bool>.Success(true, "Room soft deleted.");

                return GenericResponse<bool>.Error($"An error occurred while soft deleting room with id: {room.Id}.", StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unhandled exception happened while soft deleting room with id: {room.Id}");
                return GenericResponse<bool>.Failure($"An error occurred while soft deleting room with id: {room.Id}.");
            }
        }
        #endregion
    }
}