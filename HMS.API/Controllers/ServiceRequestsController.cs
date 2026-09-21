using HMS.Services.Abstraction;
using HMS.Shared.DTOs.ServiceModuleDTOs;
using HMS.Shared.QueryParameters.ServiceRequestModule;
using HMS.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HMS.API.Controllers
{
    public class ServiceRequestsController : BaseApiController
    {
        private readonly IRequestService _requestService;

        public ServiceRequestsController(IRequestService requestService)
        {
            _requestService = requestService;
        }

        [Authorize(Roles = "Guest")]
        [HttpPost()]
        public async Task<ActionResult<GenericResponse<bool>>> CreateServiceRequest([FromBody] CreateServiceRequestDTO requestDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _requestService.CreateServiceRequestAsync(requestDTO, userId!);

            return HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/assign")]
        public async Task<ActionResult<GenericResponse<bool>>> AssignStaffToRequest(Guid id, [FromBody] NewAssignForStaffDTO newAssign)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _requestService.AssignStaffForRequestAsync(id, adminId!, newAssign);

            return HandleResponse(result);
        }

        [Authorize(Roles = "Staff")]
        [HttpPut("status")]
        public async Task<ActionResult<bool>> UpdateRequestStatus(UpdateRequestStatusDTO updateRequest)
        {
            var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _requestService.UpdateRequestStatusAsync(staffId!, updateRequest);

            return HandleResponse(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetAllServices()
        {
            var result = await _requestService.GetAvailableServicesAsync();

            return HandleResponse(result);
        }

        [Authorize]
        [HttpGet("requests")]
        public async Task<ActionResult<IEnumerable<ServiceRequestDTO>>> GetAllServiceRequests([FromQuery] ServiceRequestQueryParams? queryParams)
        {
            var result = await _requestService.GetAllServiceRequestsAsync(queryParams);

            return HandleResponse(result);
        }
    }
}
