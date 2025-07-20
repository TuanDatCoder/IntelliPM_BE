using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ActivityLog.Request
{
    public class ActivityLogRequestDTO
    {

        public int? ProjectId { get; set; }

        public string? TaskId { get; set; }

        public string RelatedEntityType { get; set; } = null!;

        public string? RelatedEntityId { get; set; }

        public string ActionType { get; set; } = null!;

        public string? Message { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
