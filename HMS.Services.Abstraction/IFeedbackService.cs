using HMS.Shared.DTOs.ModerationModuleDTOs;
using HMS.Shared.Responses;

namespace HMS.Services.Abstraction
{
    public interface IFeedbackService
    {
        Task<GenericResponse<bool>> CreateFeedbackAsync(string userId, CreateFeedbackDTO createFeedback);
        Task<GenericResponse<IEnumerable<FeedbackDTO>>> GetAllFeedbacksAsync();
    }
}
