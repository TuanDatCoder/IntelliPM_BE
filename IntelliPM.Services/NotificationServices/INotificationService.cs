using IntelliPM.Data.DTOs.Notification.Response;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.NotificationServices
{
    public interface INotificationService
    {
        Task<List<NotificationResponseDTO>> GetNotificationByAccount(int accountId);
        Task<List<NotificationResponseDTO>> GetAllNotificationList();
        Task SendMentionNotification(List<int> mentionedUserIds, int documentId, string documentTitle, int createdBy);

        Task<List<Notification>> GetNotificationsByUserId(int userId);



    }
}
