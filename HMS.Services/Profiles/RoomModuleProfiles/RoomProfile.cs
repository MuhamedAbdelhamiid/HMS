using AutoMapper;
using HMS.Core.Entities.Enums.RoomEnums;
using HMS.Core.Entities.RoomModuleEntities;
using HMS.Services.Helpers;
using HMS.Shared.DTOs.RoomModuleDTOs;

namespace HMS.Services.Profiles.RoomModuleProfiles
{
    public class RoomProfile : Profile
    {
        public RoomProfile()
        {
            CreateMap<Room, RoomDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.RoomType.ToString()));

            CreateMap<Room, RoomDetailsDTO>().ForMember(dest => dest.Images, opt => opt.MapFrom<RoomImageValueResolver>()).ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
             .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.RoomType.ToString()));


            CreateMap<Room, AdminRoomDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.RoomType.ToString()));

            CreateMap<AdminRoomCreationDTO, Room>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => RoomStatus.Available))
                .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => RoomServiceHelper.GetRoomType(src.RoomType)));

            CreateMap<AdminRoomUpdateDTO, Room>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => RoomServiceHelper.GetRoomStatus(src.Status)))
                .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => RoomServiceHelper.GetRoomType(src.RoomType)))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now));
        }
    }
}
