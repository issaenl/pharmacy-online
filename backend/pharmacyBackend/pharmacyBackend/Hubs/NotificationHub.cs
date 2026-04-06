using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace pharmacyBackend.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}
