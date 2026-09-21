using HMS.Shared.Messages;

namespace HMS.Services.Abstraction
{
    public interface INotificationService
    {
        Task NotifyAdminsNewRequestAsync(NewRequestMessageForAdmin messageForAdmin);
        Task NotifyStaffAssignedAsync(string staffId, NewAssignForStaff newAssign);
        Task NotifyGuestStatusUpdateAsync(string userId, StatusUpdateForUser updateForUser);
    }
}
