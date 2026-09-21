using HMS.Infrastructure.ExternalServices.Hubs;
using HMS.Services.Abstraction;
using HMS.Shared.Messages;
using Microsoft.AspNetCore.SignalR;

namespace HMS.Infrastructure.ExternalServices
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<ServiceHub> _hubContext;

        public NotificationService(IHubContext<ServiceHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task NotifyAdminsNewRequestAsync(NewRequestMessageForAdmin messageForAdmin)
        => await _hubContext.Clients.Group("Admins").SendAsync("NewRequestSent", messageForAdmin);

        public async Task NotifyGuestStatusUpdateAsync(string userId, StatusUpdateForUser updateForUser)
        => await _hubContext.Clients.User(userId).SendAsync("UpdateStatusForUser", updateForUser);

        public async Task NotifyStaffAssignedAsync(string staffId, NewAssignForStaff newAssign)
        => await _hubContext.Clients.User(staffId).SendAsync("NewAssignForStaff", newAssign);
    }
}
