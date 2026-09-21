using AutoMapper;
using HMS.Core.Entities.SecurityModule;
using HMS.Shared.DTOs.SecurityModuleDTOs;

namespace HMS.Services.Profiles.SecurityModuleProfiles
{
    public class SecurityProfile : Profile
    {
        public SecurityProfile()
        {
            CreateMap<UserRegisterDTO, HotelUser>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email.Substring(0, src.Email.IndexOf('@'))));

            CreateMap<StaffCreationDTO, StaffUser>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email.Substring(0, src.Email.IndexOf('@'))))
                .ForMember(dest => dest.Specialities, opt => opt.Ignore());

            CreateMap<HotelUser, UserInfoDTO>();

            CreateMap<StaffUser, StaffDTO>()
                .ForMember(dest => dest.Specialities, opt => opt.MapFrom(src => src.Specialities.ToString()));

        }
    }
}
