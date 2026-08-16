using Microsoft.AspNetCore.SignalR;
using QuickPay.BLL.Services.Interfaces;

namespace QuickPay.PL.Hubs
{
    public class SignalRNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotifier(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PushToUserAsync(
            int userId,
            string type,
            string message,
            CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients
                .Group($"user-{userId}")
                .SendAsync(
                    "ReceiveNotification",
                    new { type, message },
                    cancellationToken);
        }
    }
}
