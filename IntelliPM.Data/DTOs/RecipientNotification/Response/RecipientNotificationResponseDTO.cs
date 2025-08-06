using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.RecipientNotification.Response
{
    public class RecipientNotificationResponseDTO
    {
        public int AccountId { get; set; }

        public string? AccountName { get; set; }

        public int NotificationId { get; set; }

        public string? NotificationMessage { get; set; }

        public string? Status { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
