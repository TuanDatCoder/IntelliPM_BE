using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Milestone.Request
{
    public class SendMilestoneEmailRequestDTO
    {
        public int ProjectId { get; set; }
        public int MilestoneId { get; set; }
     
    }
}
