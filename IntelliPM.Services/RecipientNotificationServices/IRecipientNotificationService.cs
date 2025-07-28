using IntelliPM.Data.DTOs.Notification.Response;
using IntelliPM.Data.DTOs.RecipientNotification.Response;
using IntelliPM.Data.DTOs.SubtaskComment.Response;
using IntelliPM.Data.DTOs.TaskComment.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.RecipientNotificationServices
{
    public interface IRecipientNotificationService
    {
        Task<List<RecipientNotificationResponseDTO>> GetAllRecipientNotification();

        Task<RecipientNotificationResponseDTO> GetRecipientNotificationById(int id);
        Task MarkAsReadAsync(int accountId, int notificationId);
        Task<List<RecipientNotificationResponseDTO>> GetRecipientNotificationByAccount(int accountId);
    }
}
