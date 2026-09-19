using AutoMapper;
using HMS.Core.Contracts;
using HMS.Core.Entities.BookingModule;
using HMS.Core.Entities.Enums.BookingEnums;
using HMS.Core.Entities.Enums.ServiceModule;
using HMS.Core.Entities.SecurityModule;
using HMS.Core.Entities.ServiceModule;
using HMS.Services.Abstraction;
using HMS.Services.Helpers;
using HMS.Shared.DTOs.ServiceModuleDTOs;
using HMS.Shared.Messages;
using HMS.Shared.QueryParameters.ServiceRequestModule;
using HMS.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace HMS.Services
{
    public class RequestService : IRequestService
    {
        private readonly UserManager<HotelUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly ILogger<RequestService> _logger;

        public RequestService(UserManager<HotelUser> userManager, IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, ILogger<RequestService> logger)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<GenericResponse<bool>> AssignStaffForRequestAsync(Guid requestId, string adminId, NewAssignForStaffDTO newAssign)
        {
            if (string.IsNullOrWhiteSpace(newAssign.StaffId))
                return GenericResponse<bool>.Error("Staff id cannot be null");


            var requestRepo = _unitOfWork.GetRepository<ServiceRequest, Guid>();

            var request = await requestRepo.GetByIdAsync(requestId, [r => r.Service]);

            if (request is null)
                return GenericResponse<bool>.Error("Service request not found", StatusCodes.Status404NotFound);

            if (request.Status != ServiceRequestStatus.Pending)
                return GenericResponse<bool>.Error("Cannot assign staff to not pending service request.");


            var user = await _userManager.FindByIdAsync(newAssign.StaffId);
            if (user is not StaffUser staff)
                return GenericResponse<bool>.Error($"Staff with id: {newAssign.StaffId} was not found.", StatusCodes.Status404NotFound);

            if (staff is null)
                return GenericResponse<bool>.Error($"Staff with id: {newAssign.StaffId} was not found.", StatusCodes.Status404NotFound);

            request.StaffId = newAssign.StaffId;
            request.AdminId = adminId;
            request.Status = ServiceRequestStatus.Assigned;
            request.UpdatedAt = DateTime.UtcNow;

            try
            {
                requestRepo.Update(request);
                await _unitOfWork.SaveChangesAsync();

                var assignNotification = new NewAssignForStaff()
                {
                    RequestId = request.Id,
                    RoomNumber = request.RoomNumber,
                    ServiceName = request.Service.Name
                };
                await _notificationService.NotifyStaffAssignedAsync(staff.Id, assignNotification);

                return GenericResponse<bool>.Success(true, $"Service assigned to {staff.FullName} successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unhandled error while assigning staff to request with id: {requestId}");

                return GenericResponse<bool>.Failure($"Unhandled error while assigning staff to request with id: {requestId}");
            }
        }

        public async Task<GenericResponse<bool>> CreateServiceRequestAsync(CreateServiceRequestDTO request, string userId)
        {
            var bookingRepo = _unitOfWork.GetRepository<BookingEntity, Guid>();
            var serviceRepo = _unitOfWork.GetRepository<Service, int>();
            var serviceRequestRepo = _unitOfWork.GetRepository<ServiceRequest, Guid>();

            var booking = await bookingRepo.GetByIdAsync(request.BookingId, [
                booking => booking.User,
                booking => booking.Room,
                ]);

            if (booking is null)
                return GenericResponse<bool>.Error("Booking not found.", StatusCodes.Status404NotFound);

            if (!BookingIsValid(booking))
                return GenericResponse<bool>.Error("Booking is not valid due to dates in past or not paid yet.", StatusCodes.Status402PaymentRequired);

            var service = await serviceRepo.GetByIdAsync(request.ServiceId);

            if (service is null)
                return GenericResponse<bool>.Error("Service not found.", StatusCodes.Status404NotFound);

            var serviceRequestToCreate = _mapper.Map<ServiceRequest>(request);


            serviceRequestToCreate.UserId = userId;

            try
            {
                await serviceRequestRepo.AddAsync(serviceRequestToCreate);
                await _unitOfWork.SaveChangesAsync();

                var messageForAdmins = new NewRequestMessageForAdmin()
                {
                    GuestName = booking.User.FullName,
                    Notes = serviceRequestToCreate.Notes,
                    RequestId = serviceRequestToCreate.Id,
                    RoomNumber = booking.RoomId,
                    ServiceName = service.Name
                };
                await _notificationService.NotifyAdminsNewRequestAsync(messageForAdmins);

                return GenericResponse<bool>.Success(true, "Service created successfully.", StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception happened while creating service.");

                return GenericResponse<bool>.Failure("Unhandled exception happened while creating service.");
            }
        }

        public async Task<GenericResponse<IEnumerable<ServiceRequestDTO>>> GetAllServiceRequestsAsync(ServiceRequestQueryParams? requestQueryParams)
        {
            var requestRepo = _unitOfWork.GetRepository<ServiceRequest, Guid>();

            IEnumerable<ServiceRequest> requests;

            if (requestQueryParams is not null)
            {
                var filterExp = FilterHelper.BuildFilterExpression(requestQueryParams);
                requests = await requestRepo.GetAllAsync(filter: filterExp, [s => s.Service, s => s.User]);
            }
            else
            {
                requests = await requestRepo.GetAllAsync(filter: null, [s => s.Service, s => s.User]);
            }

            if (requests is null || !requests.Any())
                return GenericResponse<IEnumerable<ServiceRequestDTO>>.Error("No request services found.", StatusCodes.Status404NotFound);

            var requestsToShow = _mapper.Map<IEnumerable<ServiceRequestDTO>>(requests);

            return GenericResponse<IEnumerable<ServiceRequestDTO>>.Success(requestsToShow, "Requests retrieved successfully.");
        }

        public async Task<GenericResponse<IEnumerable<ServiceDTO>>> GetAvailableServicesAsync()
        {
            var serviceRepo = _unitOfWork.GetRepository<Service, int>();

            var services = await serviceRepo.GetAllAsync();

            if (services is null || !services.Any())
                return GenericResponse<IEnumerable<ServiceDTO>>.Error("No services found.", StatusCodes.Status404NotFound);

            var servicesToShow = _mapper.Map<IEnumerable<ServiceDTO>>(services);

            return GenericResponse<IEnumerable<ServiceDTO>>.Success(servicesToShow, "Services retrieved successfully.");
        }

        public async Task<GenericResponse<bool>> UpdateRequestStatusAsync(string staffId, UpdateRequestStatusDTO updateRequest)
        {
            var requestRepo = _unitOfWork.GetRepository<ServiceRequest, Guid>();

            var request = await requestRepo.GetByIdAsync(updateRequest.RequestId);

            if (request is null)
                return GenericResponse<bool>.Error($"Request with id: {updateRequest.RequestId} was not found.", StatusCodes.Status404NotFound);

            if (request.StaffId != staffId)
                return GenericResponse<bool>.Error($"Request status can be updated only from staff who assigned to it.", StatusCodes.Status401Unauthorized);

            var parsed = Enum.TryParse<ServiceRequestStatus>(updateRequest.Status, out var newStatus);

            if (!parsed || (newStatus != ServiceRequestStatus.Completed &&
                            newStatus != ServiceRequestStatus.Cancelled))
            {
                return GenericResponse<bool>.Error("Invalid status. Staff can only set: Completed, or Cancelled.", StatusCodes.Status400BadRequest);
            }

            request.Status = newStatus;
            request.UpdatedAt = DateTime.UtcNow;
            try
            {
                requestRepo.Update(request);
                await _unitOfWork.SaveChangesAsync();

                var messageToUser = new StatusUpdateForUser()
                {
                    NewStatus = newStatus.ToString(),
                    RequestId = request.Id
                };
                await _notificationService.NotifyGuestStatusUpdateAsync(request.UserId, messageToUser);

                return GenericResponse<bool>.Success(true, "Status for request updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception happened while updating service request status.");

                return GenericResponse<bool>.Failure("Unhandled exception happened while updating service request status.");
            }
        }

        #region Helper Method
        private bool BookingIsValid(BookingEntity booking)
        => booking.Status == BookingStatus.Paid && booking.CheckInDate >= DateTime.UtcNow && booking.CheckOutDate > DateTime.UtcNow;
        #endregion
    }
}
