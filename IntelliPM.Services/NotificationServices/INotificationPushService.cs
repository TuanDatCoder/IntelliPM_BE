using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.NotificationServices
{
    public interface INotificationPushService
    {
        Task PushMentionNotificationAsync(int userId, string message, int documentId, string documentTitle);
    }
}
