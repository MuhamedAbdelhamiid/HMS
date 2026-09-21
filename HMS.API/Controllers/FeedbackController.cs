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

        [HttpPost]
        [Authorize(Roles = "Guest")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GenericResponse<bool>>> CreateFeedback([FromBody] CreateFeedbackDTO feedback)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _feedbackService.CreateFeedbackAsync(userId!, feedback);

            return HandleResponse(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public async Task<ActionResult<GenericResponse<IEnumerable<FeedbackDTO>>>> GetAllFeedbacks()
        {
            var result = await _feedbackService.GetAllFeedbacksAsync();

            return HandleResponse(result);
        }
    }
}
