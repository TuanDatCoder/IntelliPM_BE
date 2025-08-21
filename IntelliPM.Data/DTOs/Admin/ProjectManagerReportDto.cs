using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Admin
{
    public class ProjectManagerReportDto
    {
        public int ProjectManagerId { get; set; }
        public string ProjectManagerName { get; set; } = string.Empty;
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int OverdueTasks { get; set; }
        public decimal TotalBudget { get; set; }
        public List<ProjectSummaryDto> Projects { get; set; } = new List<ProjectSummaryDto>();
    }

    public class ProjectSummaryDto
    {
        public int ProjectId { get; set; }
        public string ProjectKey { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? Spi { get; set; }
        public decimal? Cpi { get; set; }
        public decimal Progress { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public decimal Budget { get; set; }
        public decimal ActualCost { get; set; }
        public decimal RemainingBudget { get; set; }
        public List<MilestoneSummaryDto> Milestones { get; set; } = new List<MilestoneSummaryDto>();
    }

    public class MilestoneSummaryDto
    {
        public int MilestoneId { get; set; }
        public string Key {  get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
