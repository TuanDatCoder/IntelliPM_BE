﻿using IntelliPM.Data.DTOs.WorkItemLabel.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Epic.Response
{
    public class EpicResponseDTO
    {
        public string Id { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Status { get; set; }
        public int? ReporterId { get; set; }
        public int? AssignedById { get; set; }
        public int? SprintId { get; set; }

        
    }


}
