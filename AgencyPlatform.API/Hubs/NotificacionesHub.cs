using AgencyPlatform.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AgencyPlatform.API.Hubs
{
    [Authorize]
    public class NotificacionesHub : Hub
    {
        // Método para que un usuario se una a un grupo
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // Método para que un usuario salga de un grupo
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        // Este método se ejecuta cuando un cliente se conecta
        public override async Task OnConnectedAsync()
        {
            // Puedes unir al usuario a grupos basados en sus roles/claims
            var userId = Context.User?.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            }

            await base.OnConnectedAsync();
        }
    }
}
