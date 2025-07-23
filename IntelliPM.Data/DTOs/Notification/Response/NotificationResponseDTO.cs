using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Notification.Response
{
    public class NotificationResponseDTO
    {
        public int Id { get; set; }

        public int CreatedBy { get; set; }

        public string CreatedByName { get; set; }

        public string Type { get; set; } = null!;

        public string Priority { get; set; } = null!;

        public string Message { get; set; } = null!;

        public string? RelatedEntityType { get; set; }

        public int? RelatedEntityId { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
