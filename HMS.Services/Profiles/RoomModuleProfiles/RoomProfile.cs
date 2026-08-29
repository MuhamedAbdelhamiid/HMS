using AutoMapper;
using HMS.Core.Entities.RoomModuleEntities;
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




        }
    }
}
