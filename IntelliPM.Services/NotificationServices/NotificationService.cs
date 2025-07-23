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


namespace IntelliPM.Services.NotificationServices
{
    public class NotificationService : INotificationService
    {
        private readonly IMapper _mapper;
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(IMapper mapper, INotificationRepository notificationRepository)
        {
            _mapper = mapper;
            _notificationRepository = notificationRepository;
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
        }

        public async Task<List<Notification>> GetNotificationsByUserId(int userId)
        {
            return await _notificationRepository.GetByReceiverId(userId);
        }



    }

}
