using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.RecipientNotification.Request
{
    public class RecipientNotificationRequestDTO
    {
        public int AccountId { get; set; }

        public int NotificationId { get; set; }

        public string? Status { get; set; }

    }
}
