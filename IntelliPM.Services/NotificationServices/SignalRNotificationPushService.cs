using IntelliPM.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace IntelliPM.Services.NotificationServices
{
    public class SignalRNotificationPushService : INotificationPushService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationPushService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PushMentionNotificationAsync(int userId, string message, int documentId, string documentTitle)
        {
            await _hubContext.Clients.Group(userId.ToString()).SendAsync(
    "ReceiveNotification",
    new
    {
        Message = message,
        DocumentId = documentId,
        Title = documentTitle
    });

        }

     

    }
}
