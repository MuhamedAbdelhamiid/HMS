using AutoMapper;
using HMS.Core.Contracts;
using HMS.Core.Entities.Enums.RoomEnums;
using HMS.Core.Entities.RoomModuleEntities;
using HMS.Services.Abstraction;
using HMS.Shared;
using HMS.Shared.DTOs.RoomModuleDTOs;
using HMS.Shared.Responses;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
namespace HMS.Services
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RoomService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #region Guest Services
        // apply all rooms filter and wanted parameters &
        // only shows available and reserved rooms
        public async Task<GenericResponse<IEnumerable<RoomDTO>>> GetAllRoomsAsync(
            RoomQueryParameters queryParameters)
        {
            var genericResponse =
            new GenericResponse<IEnumerable<RoomDTO>>()
            {
                StatusCode = StatusCodes.Status200OK
            };

            // get rooms repo 
            var roomRepo = _unitOfWork.GetRepository<Room, int>();

            Expression<Func<Room, object>>? orderBy, orderByDesc;
            BuildSortExpression(queryParameters, out orderBy, out orderByDesc);

            var rooms = await roomRepo.GetAllAsync(
                filter: BuildFilterExpression(queryParameters),
                orderBy: orderBy,
                orderByDesc: orderByDesc
                );

            // return all rooms 
            // check for nullability 
            if (!rooms.Any())
            {
                genericResponse.StatusCode = StatusCodes.Status404NotFound;
                genericResponse.Message = "No rooms found.";
                return genericResponse;
            }

            // map the rooms to RoomDTOs and return them
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

        #region Helper Methods
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

        private void BuildSortExpression(
            RoomQueryParameters queryParameters,
            out Expression<Func<Room, object>>? orderBy,
            out Expression<Func<Room, object>>? orderByDesc)
        {
            orderBy = null;
            orderByDesc = null;
            if (queryParameters.Sort is not null)
            {
                switch (queryParameters.Sort)
                {
                    case "priceAsc":
                        orderBy = r => r.PricePerNight;
                        break;
                    case "priceDesc":
                        orderByDesc = r => r.PricePerNight;
                        break;
                    default:
                        orderBy = r => r.Id;
                        break;
                }
            }
            else
                orderBy = r => r.Id;
        }

        private Expression<Func<Room, bool>>? BuildFilterExpression(
            RoomQueryParameters queryParameters)
            => r =>
                (r.Status == RoomStatus.Available || r.Status == RoomStatus.Reserved) &&
                (string.IsNullOrEmpty(queryParameters.RoomType) ||
                    r.RoomType == GetRoomType(queryParameters.RoomType)) &&
                (!queryParameters.MaxPrice.HasValue ||
                    r.PricePerNight <= queryParameters.MaxPrice.Value) &&
                (!queryParameters.MinPrice.HasValue ||
                    r.PricePerNight >= queryParameters.MinPrice.Value);

        private RoomType GetRoomType(string roomType)
         => roomType.ToLower() switch
         {
             "single" => RoomType.Single,
             "double" => RoomType.Double,
             "triple" => RoomType.Triple,
             _ => RoomType.Single | RoomType.Triple | RoomType.Double
         };
        #endregion
    }
}
