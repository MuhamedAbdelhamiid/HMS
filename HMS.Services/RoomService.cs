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
        // apply all rooms filter and wanted parameters &
        // only shows available and reserved rooms
        public async Task<GenericResponse<IEnumerable<RoomDTO>>> GetAllRoomsAsync(
            RoomQueryParameters? queryParameters)
        {
            var genericResponse =
            new GenericResponse<IEnumerable<RoomDTO>>()
            {
                StatusCode = StatusCodes.Status200OK
            };

            var roomRepo = _unitOfWork.GetRepository<Room, int>();

            IEnumerable<Room> rooms = null!;
            if (queryParameters is not null)
            {
                Expression<Func<Room, object>>? orderBy, orderByDesc;
                RoomServiceHelper.BuildSortExpression(queryParameters.Sort, out orderBy, out orderByDesc);

                rooms = await roomRepo.GetAllAsync(filter: RoomServiceHelper.BuildFilterExpression(queryParameters),
                   orderBy: orderBy,
                   orderByDesc: orderByDesc);
            }
            else
                rooms = await roomRepo.GetAllAsync();


            if (!rooms.Any())
            {
                genericResponse.StatusCode = StatusCodes.Status404NotFound;
                genericResponse.Message = "No rooms found.";
                return genericResponse;
            }

            var roomsToReturn =
                _mapper.Map<IEnumerable<RoomDTO>>(
                rooms);
            genericResponse.Data = roomsToReturn;
            genericResponse.Message = "Rooms retrieved successfully";

            return genericResponse;
        }

        // room exists and is its status not (in maintenance or not exist) 
        // returns room details with images
        public async Task<GenericResponse<RoomDetailsDTO>> GetRoomByIdAsync(int id)
        {
            var genericResponse =
            new GenericResponse<RoomDetailsDTO>()
            {
                StatusCode = StatusCodes.Status200OK
            };

            var roomsRepo = _unitOfWork.GetRepository<Room, int>();

            var room = await roomsRepo.GetByIdAsync(id, r => r.Images);
            genericResponse = HandleResponseBasedOnRoomInfo(id, genericResponse, room);

            return genericResponse;
        }

        #endregion

        #region Admin Services
        public async Task<GenericResponse<IEnumerable<AdminRoomDTO>>> GetAllRoomsForAdminAsync(AdminRoomQueryParameters? adminQueryParameters)
        {
            var genericResponse =
            new GenericResponse<IEnumerable<AdminRoomDTO>>()
            {
                StatusCode = StatusCodes.Status200OK
            };

            var roomRepo = _unitOfWork.GetRepository<Room, int>();

            IEnumerable<Room> rooms = null!;
            if (adminQueryParameters is not null)
            {
                Expression<Func<Room, object>>? orderBy, orderByDesc;
                RoomServiceHelper.BuildSortExpression(adminQueryParameters.Sort, out orderBy, out orderByDesc);

                rooms = await roomRepo.GetAllAsync(filter: RoomServiceHelper.BuildFilterExpression(adminQueryParameters),
                   orderBy: orderBy,
                   orderByDesc: orderByDesc);
            }
            else
                rooms = await roomRepo.GetAllAsync();


            if (rooms is null || !rooms.Any())
            {
                genericResponse.StatusCode = StatusCodes.Status404NotFound;
                genericResponse.Message = "No rooms found.";
                return genericResponse;
            }

            var roomsToReturn =
                _mapper.Map<IEnumerable<AdminRoomDTO>>(
                rooms);
            genericResponse.Data = roomsToReturn;
            genericResponse.Message = "Rooms retrieved successfully";

            return genericResponse;
        }

        public async Task<GenericResponse<bool>> CreateRoomAsync(AdminRoomCreationDTO roomToCreate)
        {
            var genericResponse = new GenericResponse<bool>()
            {
                StatusCode = StatusCodes.Status200OK
            };
            try
            {
                var roomRepo = _unitOfWork.GetRepository<Room, int>();

                if (roomToCreate is null)
                {
                    genericResponse.Message = "Invalid room data";
                    genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                    genericResponse.Data = false;
                    return genericResponse;
                }

                var roomToAdd = _mapper.Map<Room>(roomToCreate);

                roomToAdd.RoomType = (RoomType)RoomServiceHelper.GetRoomType(roomToCreate.RoomType)!;

                await roomRepo.AddAsync(roomToAdd);
                var saved = await _unitOfWork.SaveChangesAsync() > 0;

                if (!saved)
                {
                    genericResponse.Message = "Failed to create the room.";
                    genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                    genericResponse.Data = false;
                }

                genericResponse.Message = "Room created successfully.";
                genericResponse.Data = true;

                return genericResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating room.");

                genericResponse.Message = "An error occurred while creating the room.";
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Data = false;
                return genericResponse;
            }
        }

        public async Task<GenericResponse<bool>> UpdateRoomAsync(int id, AdminRoomUpdateDTO roomToUpdate)
        {
            // get the room with specified id and check for update validation
            // => room exists and is not in maintenance or not exist and available, check if there any room with the choosen specifications
            var genericResponse = new GenericResponse<bool>()
            {
                StatusCode = StatusCodes.Status200OK
            };
            try
            {
                var roomRepo = _unitOfWork.GetRepository<Room, int>();

                var room = await roomRepo.GetByIdAsync(id, r => r.Images);

                if (room is null)
                {
                    genericResponse.Message = "Room can not be updated cause it was not found.";
                    genericResponse.StatusCode = StatusCodes.Status404NotFound;
                    genericResponse.Data = false;
                    return genericResponse;
                }

                room = _mapper.Map(roomToUpdate, room);
                roomRepo.Update(room);
                var saved = await _unitOfWork.SaveChangesAsync() > 0;

                if (saved)
                {
                    genericResponse.Message = "Room updated successfully.";
                    genericResponse.StatusCode = StatusCodes.Status200OK;
                    genericResponse.Data = true;
                }
                else
                {
                    genericResponse.Message = "Error while room update.";
                    genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                    genericResponse.Data = false;
                }
                return genericResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There an unexpected error happened while updating on room with id: {id}.");
                genericResponse.Message = $"There an unexpected error happened while updating on room with id: {id}.";
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Data = false;
                return genericResponse;
            }
        }

        public async Task<GenericResponse<bool>> DeleteRoomAsync(int id)
        {
            var genericResponse = new GenericResponse<bool>()
            {
                StatusCode = StatusCodes.Status200OK
            };

            var roomRepo = _unitOfWork.GetRepository<Room, int>();

            // Not Completed Logic => we need to get the bookings in future
            var room = await roomRepo.GetByIdAsync(id);

            if (room is null)
            {
                genericResponse.StatusCode = StatusCodes.Status404NotFound;
                genericResponse.Message = $"Room with id: {id} was not found.";
                return genericResponse;
            }

            return await HandleDeleteDecision(room, genericResponse, roomRepo);
        }

        public async Task<GenericResponse<bool>> UploadRoomImageAsync(int roomId, List<IFormFile> imageFiles)
        {
            var genericResponse = new GenericResponse<bool>()
            {
                StatusCode = StatusCodes.Status200OK
            };

            var roomRepo = _unitOfWork.GetRepository<Room, int>();

            try
            {
                // get the room loaded and check if it exists 
                var room = await roomRepo.GetByIdAsync(roomId, r => r.Images);

                if (room is null)
                {
                    genericResponse.StatusCode = StatusCodes.Status404NotFound;
                    genericResponse.Message = $"Room with id: {roomId} was not found.";
                    return genericResponse;
                }

                if (imageFiles is null || !imageFiles.Any())
                {
                    genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                    genericResponse.Message = $"No images sent.";
                    return genericResponse;
                }

                // upload the images files to the server and get the paths
                var uploadedImagePaths = new List<string>();
                var imageUrls = new List<RoomImage>();
                foreach (var image in imageFiles)
                {
                    var filePath = await _attachmentService.UploadAsync("images", "rooms", image);
                    // check for the paths
                    if (string.IsNullOrEmpty(filePath))
                    {
                        foreach (var imagePath in uploadedImagePaths)
                            _attachmentService.Delete(imagePath);

                        genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                        genericResponse.Message = $"Failed to upload image: {image.FileName}";
                        return genericResponse;
                    }
                    else
                    {
                        imageUrls.Add(new RoomImage()
                        {
                            ImageUrl = filePath,
                        });
                        uploadedImagePaths.Add(filePath);
                    }
                }
                // then add the paths to the images section in room 

                foreach (var image in imageUrls)
                    room.Images.Add(image);

                room.UpdatedAt = DateTime.Now;
                roomRepo.Update(room);
                var saved = await _unitOfWork.SaveChangesAsync() > 0;
                if (saved)
                {
                    genericResponse.Message = $"Images uploaded successfully.";
                    genericResponse.Data = true;
                    return genericResponse;
                }

                foreach (var imagePath in uploadedImagePaths)
                    _attachmentService.Delete(imagePath);

                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Message = $"Failed to upload images.";
                return genericResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while uploading room images for room id: {roomId}");
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Message = $"Failed to upload images.";
                return genericResponse;
            }
        }

        public async Task<GenericResponse<bool>> DeleteRoomImagesAsync(int roomId, int imageId)
        {
            // get the room and check if it exists
            var genericResponse = new GenericResponse<bool>()
            {
                StatusCode = StatusCodes.Status200OK
            };

            var roomRepo = _unitOfWork.GetRepository<Room, int>();

            try
            {
                // get the room loaded and check if it exists 
                var room = await roomRepo.GetByIdAsync(roomId, r => r.Images);

                if (room is null || !room.Images.Any())
                {
                    genericResponse.StatusCode = StatusCodes.Status404NotFound;
                    genericResponse.Message = $"Room with id: {roomId} was not found or have no images yet.";
                    return genericResponse;
                }


                var imageExists = room.Images.Any(img => img.Id == imageId);

                if (!imageExists)
                {
                    genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                    genericResponse.Message = $"There is file paths not exist for this room with id: {roomId}.";
                    return genericResponse;
                }

                var roomImage = room.Images.FirstOrDefault(
                    img => img.Id == imageId);

                if (roomImage is null)
                {
                    genericResponse.Message = $"There is no image id: {imageId} for room with id: {roomId}.";
                    genericResponse.StatusCode = StatusCodes.Status404NotFound;
                    genericResponse.Data = false;
                    return genericResponse;
                }

                room.Images.Remove(roomImage);

                var saved = await _unitOfWork.SaveChangesAsync() > 0;
                if (saved)
                {
                    _attachmentService.Delete(roomImage!.ImageUrl);
                    genericResponse.Message = $"Images deleted successfully.";
                    genericResponse.Data = true;
                    return genericResponse;
                }
                else
                {
                    genericResponse.Message = $"Error occurred while saving deleting image with id: {imageId} for room id: {roomId}.";
                    genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                    genericResponse.Data = false;
                    return genericResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while saving deleting image with id: {imageId} for room id: {roomId}.");
                genericResponse.Message = $"Error occurred while saving deleting image with id: {imageId} for room id: {roomId}.";
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Data = false;
                return genericResponse;
            }
        }

        #endregion

        #region Helper Methods
        // They needs mapper and unit of work, So that is why they are not in RoomServiceHelper


        private GenericResponse<RoomDetailsDTO> HandleResponseBasedOnRoomInfo(
    int id, GenericResponse<RoomDetailsDTO> genericResponse, Room? room)
        {
            if (room is null || room.Status == RoomStatus.NotExist)
            {
                genericResponse.StatusCode = StatusCodes.Status404NotFound;
                genericResponse.Message = $"No room found with the specified id: {id}";
                return genericResponse;
            }

            if (room.Status == RoomStatus.Maintenance)
            {
                genericResponse.StatusCode = StatusCodes.Status404NotFound;
                genericResponse.Message = $"Room with id: {id} in maintenance.";
                return genericResponse;
            }

            var roomToReturn = _mapper.Map<RoomDetailsDTO>(room);
            genericResponse.Data = roomToReturn;
            genericResponse.Message = $"Room with id: {id}, retrieved successfully";
            return genericResponse;
        }

        private async Task<GenericResponse<bool>> HandleDeleteDecision(Room room, GenericResponse<bool> genericResponse, IGenericRepository<Room, int> roomRepo)
        {
            switch (room.Status)
            {
                // check if it is available or in maintinance => it's ok to be soft deleted 
                case RoomStatus.Available or RoomStatus.Maintenance:
                    return await SoftDeleteRoom(room, genericResponse, roomRepo);
                // reserved or not exist at all ! => no
                case RoomStatus.Reserved:
                    genericResponse.StatusCode = StatusCodes.Status400BadRequest;
                    genericResponse.Message = $"Room with id: {room.Id} is reserved and can not be deleted.";
                    genericResponse.Data = false;
                    return genericResponse;
                // not exist ? => already soft deleted 
                case RoomStatus.NotExist:
                    genericResponse.StatusCode = StatusCodes.Status200OK; genericResponse.Message = $"Room with id: {room.Id} already soft deleted.";
                    genericResponse.Data = true;
                    return genericResponse;
                default:
                    genericResponse
                        .StatusCode = StatusCodes.Status500InternalServerError;
                    genericResponse.Message = $"Unknow room status.";
                    return genericResponse;
            }
        }

        private async Task<GenericResponse<bool>> SoftDeleteRoom(Room room, GenericResponse<bool> genericResponse, IGenericRepository<Room, int> roomRepo)
        {
            room.Status = RoomStatus.NotExist;
            room.UpdatedAt = DateTime.Now;
            try
            {
                roomRepo.Update(room);
                var saved = await _unitOfWork.SaveChangesAsync() > 0;
                if (saved)
                {
                    genericResponse.StatusCode = StatusCodes.Status200OK;
                    genericResponse.Data = true;
                    genericResponse.Message = "Room soft deleted.";
                    return genericResponse;
                }
                else
                {
                    genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                    genericResponse.Data = false;
                    genericResponse.Message = $"An error occurred while soft deleting room with id: {room.Id}.";
                    return genericResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unhandled exception happened while soft deleting room with id: {room.Id}");
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Data = false;
                genericResponse.Message = $"An error occurred while soft deleting room with id: {room.Id}.";
                return genericResponse;
            }
        }


        #endregion
    }
}
