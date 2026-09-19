using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace HMS.Infrastructure.ExternalServices.Hubs
{
    public class ServiceHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var role = Context.User!.FindFirstValue(ClaimTypes.Role);

            if (!string.IsNullOrWhiteSpace(role) && role.ToLower() == "admin")
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");

            await base.OnConnectedAsync();
        }
    }
}
