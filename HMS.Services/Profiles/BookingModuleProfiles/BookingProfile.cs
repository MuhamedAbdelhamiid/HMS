using AutoMapper;
using HMS.Core.Entities.BookingModule;
using HMS.Shared.DTOs.BookingModuleDTOs;

namespace HMS.Services.Profiles.BookingModuleProfiles
{
    public class BookingProfile : Profile
    {
        public BookingProfile()
        {
            CreateMap<CreateBookingDTO, BookingEntity>()
                .ForMember(dest => dest.HotelUserId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore());
        }
    }
}
