using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectMetric.Response
{
    public class CostDashboardResponseDTO
    {
        public decimal ActualCost { get; set; }
        public decimal ActualTaskCost { get; set; }
        public decimal ActualResourceCost { get; set; }

        public decimal PlannedCost { get; set; }
        public decimal PlannedTaskCost { get; set; }
        public decimal PlannedResourceCost { get; set; }

        public decimal EarnedValue { get; set; }

        public decimal Budget { get; set; }
    }

}
