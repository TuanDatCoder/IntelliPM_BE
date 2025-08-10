using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ActivityLog.Response
{
    public class ActivityLogResponseDTO
    {
        public int Id { get; set; }

        public int? ProjectId { get; set; }

        public string? TaskId { get; set; }

        public string? SubtaskId { get; set; }

        public string? RiskKey { get; set; }

        public string RelatedEntityType { get; set; } = null!;

        public string? RelatedEntityId { get; set; }

        public string ActionType { get; set; } = null!;

        public string? FieldChanged { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public string? Message { get; set; }

        public int CreatedBy { get; set; }

        public string CreatedByName { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
