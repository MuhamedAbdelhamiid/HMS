using HMS.Shared.DTOs.ServiceModuleDTOs;
using HMS.Shared.Responses;

namespace HMS.Services.Abstraction
{
    public interface IRequestService
    {
        Task<GenericResponse<bool>> CreateServiceRequestAsync(CreateServiceRequestDTO request, string userId);
        Task<GenericResponse<bool>> AssignStaffForRequestAsync(Guid requestId, string adminId, NewAssignForStaffDTO newAssign);
        Task<GenericResponse<bool>> UpdateRequestStatusAsync(string staffId, UpdateRequestStatusDTO updateRequest);
        Task<GenericResponse<IEnumerable<ServiceRequestDTO>>> GetAllServiceRequestsAsync();
        Task<GenericResponse<IEnumerable<ServiceDTO>>> GetAvailableServicesAsync();
    }
}
