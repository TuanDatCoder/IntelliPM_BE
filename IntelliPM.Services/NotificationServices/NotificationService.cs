using AutoMapper;
using IntelliPM.Data.DTOs.Notification.Response;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
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
    }
}
