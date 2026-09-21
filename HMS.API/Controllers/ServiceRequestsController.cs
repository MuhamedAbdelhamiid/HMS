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

        [HttpPost()]
        [Authorize(Roles = "Guest")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<GenericResponse<bool>>> CreateServiceRequest([FromBody] CreateServiceRequestDTO requestDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _requestService.CreateServiceRequestAsync(requestDTO, userId!);

            return HandleResponse(result);
        }

        [HttpPut("{id}/assign")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<GenericResponse<bool>>> AssignStaffToRequest(Guid id, [FromBody] NewAssignForStaffDTO newAssign)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _requestService.AssignStaffForRequestAsync(id, adminId!, newAssign);

            return HandleResponse(result);
        }

        [HttpPut("status")]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> UpdateRequestStatus(UpdateRequestStatusDTO updateRequest)
        {
            var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _requestService.UpdateRequestStatusAsync(staffId!, updateRequest);

            return HandleResponse(result);
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetAllServices()
        {
            var result = await _requestService.GetAvailableServicesAsync();

            return HandleResponse(result);
        }

        [Authorize]
        [HttpGet("requests")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ServiceRequestDTO>>> GetAllServiceRequests([FromQuery] ServiceRequestQueryParams? queryParams)
        {
            var result = await _requestService.GetAllServiceRequestsAsync(queryParams);

            return HandleResponse(result);
        }
    }
}
