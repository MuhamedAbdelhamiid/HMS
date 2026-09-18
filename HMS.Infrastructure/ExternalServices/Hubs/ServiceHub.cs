using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace HMS.Infrastructure.ExternalServices.Hubs
{
    public class ServiceHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            var role = Context.User!.FindFirstValue(ClaimTypes.Role);

            if (!string.IsNullOrWhiteSpace(role) && role.ToLower() == "admin")
                Groups.AddToGroupAsync(Context.ConnectionId, "Admins");

            return base.OnConnectedAsync();
        }
    }
}
