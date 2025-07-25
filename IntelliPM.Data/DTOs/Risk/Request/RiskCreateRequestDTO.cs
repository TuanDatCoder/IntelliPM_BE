﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Risk.Request
{
    public class RiskCreateRequestDTO
    {
        public string ProjectKey { get; set; } = null!;
        public int? ResponsibleId { get; set; }
        public string? TaskId { get; set; }
        public string RiskScope { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public string? GeneratedBy { get; set; }
        public string? Probability { get; set; }
        public string? ImpactLevel { get; set; }
        public string? SeverityLevel { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
