using IntelliPM.Data.DTOs.Notification.Response;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
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
    }
}
