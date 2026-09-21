using AutoMapper;
using HMS.Core.Contracts;
using HMS.Core.Entities.BookingModule;
using HMS.Core.Entities.ModerationModule;
using HMS.Services.Abstraction;
using HMS.Services.Helpers;
using HMS.Shared.DTOs.ModerationModuleDTOs;
using HMS.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HMS.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IAiModerationService _moderationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<FeedbackService> _logger;

        public FeedbackService(IAiModerationService moderationService, IUnitOfWork unitOfWork, IMapper mapper, ILogger<FeedbackService> logger)
        {
            _moderationService = moderationService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<GenericResponse<bool>> CreateFeedbackAsync(string userId, CreateFeedbackDTO createFeedback)
        {
            var feedbackRepo = _unitOfWork.GetRepository<Feedback, Guid>();
            var bookingRepo = _unitOfWork.GetRepository<BookingEntity, Guid>();

            var booking = await bookingRepo.GetByIdAsync(createFeedback.BookingId);

            if (booking is null)
                return GenericResponse<bool>.Error("Booking not found.", StatusCodes.Status404NotFound);

            if (!BookingServiceHelper.BookingIsValid(booking))
                return GenericResponse<bool>.Error("Booking not valid to create a feedback for.", StatusCodes.Status402PaymentRequired);

            if (!(booking.HotelUserId == userId))
                return GenericResponse<bool>.Error("User unauthorized to feedback this booking.", StatusCodes.Status401Unauthorized);

            if (string.IsNullOrWhiteSpace(createFeedback.Comment))
                return GenericResponse<bool>.Error("Comment can not be empty", StatusCodes.Status400BadRequest);

            var commentIsAbleToBeStored = await _moderationService.IsUserCommentAcceptedAsFeedbackAsync(createFeedback.Comment);

            if (!commentIsAbleToBeStored)
                return GenericResponse<bool>.Error("Comment not accepted as feedback.", StatusCodes.Status400BadRequest);

            var feedbackToBeCreate = _mapper.Map<Feedback>(createFeedback);

            try
            {
                await feedbackRepo.AddAsync(feedbackToBeCreate);
                await _unitOfWork.SaveChangesAsync();

                return GenericResponse<bool>.Success(true, "Feedback created successfully.", StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception happened while saving feedback in database.");

                return GenericResponse<bool>.Failure("Unhandled exception happened while saving feedback in database.");
            }
        }

        public async Task<GenericResponse<IEnumerable<FeedbackDTO>>> GetAllFeedbacksAsync()
        {
            var feedbackRepo = _unitOfWork.GetRepository<Feedback, Guid>();

            var feedbacks = await feedbackRepo.GetAllAsync();

            if (feedbacks is null || !feedbacks.Any())
                return GenericResponse<IEnumerable<FeedbackDTO>>.Error("Feedbacks not found.", StatusCodes.Status404NotFound);

            var feedbacksToShow = _mapper.Map<IEnumerable<FeedbackDTO>>(feedbacks);

            return GenericResponse<IEnumerable<FeedbackDTO>>.Success(feedbacksToShow, "Feedbacks retrieved successfully.");
        }
    }
}
