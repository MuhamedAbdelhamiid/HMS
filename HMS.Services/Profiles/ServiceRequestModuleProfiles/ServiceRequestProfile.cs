using AutoMapper;
using HMS.Core.Entities.ServiceModule;
using HMS.Shared.DTOs.ServiceModuleDTOs;

namespace HMS.Services.Profiles.ServiceRequestModuleProfiles
{
    public class ServiceRequestProfile : Profile
    {
        public ServiceRequestProfile()
        {
            CreateMap<CreateServiceRequestDTO, ServiceRequest>();

            CreateMap<ServiceRequest, ServiceRequestDTO>()
                .ForMember(dest => dest.RoomNumber, opt => opt.Ignore())
                .ForMember(dest => dest.GuestId, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.Name));

            CreateMap<Service, ServiceDTO>();
        }
    }
}
