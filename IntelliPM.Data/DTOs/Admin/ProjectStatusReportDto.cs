using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Admin
{
    public class ProjectStatusReportDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectKey { get; set; }
        public string ProjectManager { get; set; }

        public decimal SPI { get; set; }
        public decimal CPI { get; set; }

        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public decimal Progress { get; set; }

        public decimal? ActualCost { get; set; }
        public decimal? Budget { get; set; }
        public decimal? RemainingBudget => Budget - ActualCost;

        public List<MilestoneDto> Milestones { get; set; }
        public int OverdueTasks { get; set; }
    }

    public class MilestoneDto
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

}
