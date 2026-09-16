using AutoMapper;
using HMS.Core.Entities.RoomModule;
using HMS.Shared.DTOs.RoomModuleDTOs;
using Microsoft.Extensions.Configuration;

namespace HMS.Services.Profiles.RoomModuleProfiles
{
    public class RoomImageValueResolver : IValueResolver<Room, RoomDetailsDTO, ICollection<RoomImageDTO>>
    {
        private readonly IConfiguration _configuration;

        public RoomImageValueResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public ICollection<RoomImageDTO> Resolve(
            Room source,
            RoomDetailsDTO destination,
           ICollection<RoomImageDTO> destMember,
            ResolutionContext context)
        {

            var returnedImages = new List<RoomImageDTO>();
            var baseUrl = _configuration.GetSection("Urls")["BaseUrl"];
            foreach (var image in source.Images)
            {
                if (string.IsNullOrEmpty(image.ImageUrl))
                    continue;
                else if (image.ImageUrl.ToLower().StartsWith("http")
                    || image.ImageUrl.ToLower().StartsWith("https"))
                    returnedImages.Add(new RoomImageDTO()
                    {
                        ImageUrl = image.ImageUrl,
                        Id = image.Id
                    });
                else
                    returnedImages.Add(new RoomImageDTO()
                    {
                        ImageUrl = $"{baseUrl}/{image.ImageUrl}",
                        Id = image.Id
                    });
            }

            return returnedImages;
        }
    }
}
