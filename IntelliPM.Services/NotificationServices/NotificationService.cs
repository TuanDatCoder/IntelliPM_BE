using IntelliPM.Data.Entities;
using IntelliPM.Repositories.NotificationRepos;

using IntelliPM.Services.NotificationServices;
using IntelliPM.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace IntelliPM.Services.NotificationServices
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationPushService _pushService;

        public NotificationService(INotificationRepository notificationRepository, INotificationPushService pushService)
        {
            _notificationRepository = notificationRepository;
            _pushService = pushService;

        }

        public async Task SendMentionNotification(List<int> mentionedUserIds, int documentId, string documentTitle, int createdBy)
        {
            if (mentionedUserIds == null || !mentionedUserIds.Any()) return;

            var notification = new Notification

            {
                CreatedBy = createdBy,
                Type = "MENTION",
                Priority = "NORMAL",
                Message = $"You were mentioned in document \"{documentTitle}\"",
                RelatedEntityType = "DOCUMENT",
                RelatedEntityId = documentId,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var userId in mentionedUserIds.Distinct())
            {
                notification.RecipientNotification.Add(new RecipientNotification
                {
                    AccountId = userId,
                    IsRead = false,
                    Status = "RECEIVED",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _notificationRepository.Add(notification);

            foreach (var userId in mentionedUserIds.Distinct())
            {
                await _pushService.PushMentionNotificationAsync(userId, notification.Message, documentId, documentTitle);
            }
        }

        public async Task<List<Notification>> GetNotificationsByUserId(int userId)
        {
            return await _notificationRepository.GetByReceiverId(userId);
        }

    }

}
