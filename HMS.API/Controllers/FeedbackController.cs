using HMS.Services.Abstraction;
using HMS.Shared.DTOs.ModerationModuleDTOs;
using HMS.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HMS.API.Controllers
{
    public class FeedbackController : BaseApiController
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [Authorize(Roles = "Guest")]
        [HttpPost]
        public async Task<ActionResult<GenericResponse<bool>>> CreateFeedback([FromBody] CreateFeedbackDTO feedback)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _feedbackService.CreateFeedbackAsync(userId!, feedback);

            return HandleResponse(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<GenericResponse<IEnumerable<FeedbackDTO>>>> GetAllFeedbacks()
        {
            var result = await _feedbackService.GetAllFeedbacksAsync();

            return HandleResponse(result);
        }
    }
}
