using AutoMapper;
using HMS.Core.Entities.BookingModule;
using HMS.Shared.DTOs.BookingModuleDTOs;
using HMS.Shared.DTOs.SecurityModuleDTOs;

namespace HMS.Services.Profiles.BookingModuleProfiles
{
    public class BookingProfile : Profile
    {
        public BookingProfile()
        {
            CreateMap<CreateBookingDTO, BookingEntity>()
                .ForMember(dest => dest.HotelUserId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore());

            CreateMap<BookingEntity, UserBookingDTO>()
                .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.Status.ToString()));


        }
    }
}
