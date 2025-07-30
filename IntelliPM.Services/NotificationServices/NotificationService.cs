using AutoMapper;
using IntelliPM.Data.DTOs.Notification.Response;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.NotificationRepos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IntelliPM.Services.NotificationServices;
using IntelliPM.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace IntelliPM.Services.NotificationServices
{
    public class NotificationService : INotificationService
    {
        private readonly IMapper _mapper;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationPushService _pushService;

        public NotificationService(IMapper mapper, INotificationRepository notificationRepository, INotificationPushService pushService)
        {
            _mapper = mapper;
            _notificationRepository = notificationRepository;
            _pushService = pushService;

        }

        public async Task<List<NotificationResponseDTO>> GetAllNotificationList()
        {
            var entities = await _notificationRepository.GetAllNotification();
            return _mapper.Map<List<NotificationResponseDTO>>(entities);
        }

        public async Task<List<NotificationResponseDTO>> GetNotificationByAccount(int accountId)
            {
            var entity = await _notificationRepository.GetNotificationByAccountIdAsync(accountId);
            if (entity == null)
                throw new KeyNotFoundException($"Notification with Account {accountId} not found.");

            return _mapper.Map<List<NotificationResponseDTO>>(entity);
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
        public async Task SendMeetingNotification(List<int> participantIds, int meetingId, string meetingTopic, int createdBy)
        {
            if (participantIds == null || !participantIds.Any()) return;

            var message = $"You have been invited to a meeting: {meetingTopic}";

            var notification = new Notification
            {
                CreatedBy = createdBy,
                Type = "MEETING",
                Priority = "NORMAL",
                Message = message,
                RelatedEntityType = "MEETING",
                RelatedEntityId = meetingId,
                CreatedAt = DateTime.UtcNow,
                RecipientNotification = new List<RecipientNotification>()
            };

            foreach (var userId in participantIds.Distinct())
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

            foreach (var userId in participantIds.Distinct())
            {
                await _pushService.PushMentionNotificationAsync(userId, message, meetingId, meetingTopic);
            }
        }



    }

}
