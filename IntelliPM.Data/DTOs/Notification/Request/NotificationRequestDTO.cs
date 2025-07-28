using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Notification.Request
{
    public class NotificationRequestDTO
    {
        public int CreatedBy { get; set; }

        public string Type { get; set; } = null!;

        public string Priority { get; set; } = null!;

        public string Message { get; set; } = null!;

        public string? RelatedEntityType { get; set; }

        public int? RelatedEntityId { get; set; }
    }
}
