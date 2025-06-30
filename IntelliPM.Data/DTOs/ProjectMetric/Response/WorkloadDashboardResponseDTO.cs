using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectMetric.Response
{
    public class WorkloadDashboardResponseDTO
    {
        public string MemberName { get; set; }
        public int Completed { get; set; }
        public int Remaining { get; set; }
        public int Overdue { get; set; }
    }
}
