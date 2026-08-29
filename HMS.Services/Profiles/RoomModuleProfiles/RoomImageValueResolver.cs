using AutoMapper;
using HMS.Core.Entities.RoomModuleEntities;
using HMS.Shared.DTOs.RoomModuleDTOs;
using Microsoft.Extensions.Configuration;

namespace HMS.Services.Profiles.RoomModuleProfiles
{
    public class RoomImageValueResolver : IValueResolver<Room, RoomDetailsDTO, ICollection<string>>
    {
        private readonly IConfiguration _configuration;

        public RoomImageValueResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public ICollection<string> Resolve(
            Room source,
            RoomDetailsDTO destination,
            ICollection<string> destMember,
            ResolutionContext context)
        {

            var returnedImages = new List<string>();
            var baseUrl = _configuration.GetSection("Urls")["BaseUrl"];
            foreach (var image in source.Images)
            {
                if (string.IsNullOrEmpty(image.ImageUrl))
                    continue;
                else if (image.ImageUrl.ToLower().StartsWith("http")
                    || image.ImageUrl.ToLower().StartsWith("https"))
                    returnedImages.Add(image.ImageUrl);
                else
                    returnedImages.Add($"{baseUrl}/{image.ImageUrl}");
            }

            return returnedImages;
        }
    }
}
