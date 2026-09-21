using AutoMapper;
using HMS.Core.Entities.ModerationModule;
using HMS.Shared.DTOs.ModerationModuleDTOs;

namespace HMS.Services.Profiles.ModerationModuleProfiles
{
    public class ModerationProfile : Profile
    {
        public ModerationProfile()
        {
            CreateMap<CreateFeedbackDTO, Feedback>();

            CreateMap<Feedback, FeedbackDTO>();
        }
    }
}
